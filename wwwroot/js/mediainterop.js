let mediaRecorder;
let onDataCallback;

function startAudioRecording(callback) {
    onDataCallback = callback;

    navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
        mediaRecorder = new MediaRecorder(stream, { mimeType: "audio/webm" });

        mediaRecorder.ondataavailable = async (e) => {
            if (e.data.size > 0) {
                const arrayBuffer = await e.data.arrayBuffer();
                const uint8Array = new Uint8Array(arrayBuffer);
                const base64String = btoa(String.fromCharCode(...uint8Array));
                callback(base64String);
            }
        };

        mediaRecorder.start(250);
    });
}


function startAudioRecording(callback) {
    onDataCallback = callback;

    navigator.mediaDevices.getUserMedia({
        audio: {
            sampleRate: 16000,
            echoCancellation: true,
            noiseSuppression: true
        }
    }).then(stream => {
        const mimeTypes = [
            'audio/webm;codecs=opus',
            'audio/webm',
            'audio/mp4',
            'audio/ogg;codecs=opus'
        ];

        let selectedMimeType = null;
        for (const mimeType of mimeTypes) {
            if (MediaRecorder.isTypeSupported(mimeType)) {
                selectedMimeType = mimeType;
                console.log(`[Voice] Используется MIME-тип: ${mimeType}`);
                break;
            }
        }

        if (!selectedMimeType) {
            console.error("[Voice] Не найден поддерживаемый MIME-тип");
            return;
        }

        mediaRecorder = new MediaRecorder(stream, {
            mimeType: selectedMimeType,
            audioBitsPerSecond: 16000
        });

        mediaRecorder.ondataavailable = async (e) => {
            if (e.data.size > 0) {
                console.log(`[Voice] Размер аудиочанка: ${e.data.size} байт`);

                if (e.data.size < 1000) {
                    console.log("[Voice] Чанк слишком маленький, пропускаем");
                    return;
                }

                const arrayBuffer = await e.data.arrayBuffer();
                const uint8Array = new Uint8Array(arrayBuffer);
                const base64String = btoa(String.fromCharCode(...uint8Array));
                callback(base64String);
            }
        };

        mediaRecorder.start(1000);
    }).catch(error => {
        console.error("[Voice] Ошибка доступа к микрофону:", error);
    });
}

async function playAudio(base64Audio) {
    try {
        if (!base64Audio || base64Audio.length === 0) {
            console.warn("[Voice] Получена пустая строка base64");
            return;
        }

        const binary = atob(base64Audio);
        const byteArray = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            byteArray[i] = binary.charCodeAt(i);
        }

        const blob = new Blob([byteArray], { type: "audio/webm;codecs=opus" });

        if (blob.size === 0) {
            console.warn("[Voice] Blob пустой, пропускаем воспроизведение");
            return;
        }

        console.log(`[Voice] Создан blob размером ${blob.size} байт`);

        const url = URL.createObjectURL(blob);
        const audio = new Audio(url);

        audio.onerror = (e) => {
            console.error("[Voice] Ошибка воспроизведения:", e);
            URL.revokeObjectURL(url);
        };

        audio.onended = () => {
            URL.revokeObjectURL(url);
        };

        await audio.play();

    } catch (error) {
        console.error("[Voice] Ошибка в playAudio:", error);
    }
}


