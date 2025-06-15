// Channels.js (финальная версия под base64)
let currentChannelId = null;
let currentUser = document.getElementById("current-username")?.value ?? "Аноним";

let connection = new signalR.HubConnectionBuilder()
    .withUrl("/voicehub")
    .build();

connection.on("UpdateUserList", function (channelId, users) {
    const list = document.getElementById("users-" + channelId);
    if (!list) return;

    list.innerHTML = "";
    console.log("[Voice] UpdateUserList получен ", users);
    users.forEach(user => {
        const li = document.createElement("li");
        li.innerHTML = generateAvatar(user) + " " + user;
        if (user === currentUser) li.classList.add("self-user");
        list.appendChild(li);
    });
});

connection.on("ReceiveAudio", async (senderId, base64Audio) => {
    await playAudio(base64Audio);
});

async function connectToChannel(channelId) {
    if (currentChannelId && currentChannelId !== channelId) {
        await disconnectFromChannel(currentChannelId);
        const oldList = document.getElementById("users-" + currentChannelId);
        if (oldList) oldList.innerHTML = "";
    }

    currentChannelId = channelId;

    if (connection.state !== "Connected") {
        try {
            await connection.start();
            console.log("[Voice] ✅ SignalR подключен");
        } catch (err) {
            console.error("[Voice ERROR] SignalR failed:", err);
            return;
        }
    }

    await connection.invoke("JoinChannel", channelId.toString());
    console.log("[Voice] Присоединение к каналу:", channelId);
    highlightActiveChannel(channelId);
}

async function disconnectFromChannel(channelId) {
    if (connection && connection.state === "Connected") {
        await connection.invoke("LeaveChannel", channelId.toString());
    }
    hideMicIndicator();
}

async function startRecording() {
    highlightUserSpeaking(currentChannelId, currentUser);
    showMicIndicator();
    console.log("[Voice] Запуск записи для канала:", currentChannelId);

    if (!currentChannelId) {
        console.error("[Voice ERROR] Не выбран канал для записи");
        return;
    }

    startAudioRecording(async (base64String) => {
        console.log("[Voice] 🎧 Отправка аудио, размер base64:", base64String.length);
        try {
            if (connection.state !== signalR.HubConnectionState.Connected) {
                console.warn("[Voice] SignalR не подключен, пропускаем отправку");
                return;
            }

            await connection.invoke("SendAudio", currentChannelId.toString(), base64String);
        } catch (err) {
            console.error("[Voice ERROR] SendAudio error:", err);
            console.error("[Voice ERROR] Детали ошибки:", err.message);
        }
    });
}


function stopRecording() {
    stopAudioRecording();
    hideMicIndicator();
    document.querySelectorAll("li.speaking").forEach(li => li.classList.remove("speaking"));
}

function highlightUserSpeaking(channelId, username) {
    const liList = document.querySelectorAll(`#users-${channelId} li`);
    liList.forEach(li => {
        if (li.textContent.includes(username)) {
            li.classList.add('speaking');
        } else {
            li.classList.remove('speaking');
        }
    });
}

function highlightActiveChannel(channelId) {
    document.querySelectorAll("button[id^='join-btn-']").forEach(btn => {
        btn.classList.remove("active-channel");
    });

    const btn = document.getElementById("join-btn-" + channelId);
    if (btn) btn.classList.add("active-channel");
}

function generateAvatar(username) {
    const color = '#' + intToRGB(hashCode(username));
    const initial = username[0].toUpperCase();
    return `<span class="avatar" style="background:${color}">${initial}</span>`;
}

function hashCode(str) {
    return str.split('').reduce((acc, c) => acc + c.charCodeAt(0), 0);
}

function intToRGB(i) {
    return ((i & 0x00FFFFFF).toString(16).toUpperCase()).padStart(6, '0');
}

function showMicIndicator() {
    const indicator = document.getElementById("mic-indicator");
    if (indicator) indicator.style.display = "block";
}

function hideMicIndicator() {
    const indicator = document.getElementById("mic-indicator");
    if (indicator) indicator.style.display = "none";
}