
function checkAudioSupport() {
    const audio = new Audio();
    const formats = {
        webm: audio.canPlayType('audio/webm;codecs=opus'),
        mp4: audio.canPlayType('audio/mp4'),
        ogg: audio.canPlayType('audio/ogg;codecs=opus')
    };

    console.log("[Voice] Поддержка форматов:", formats);
    return formats;
}


document.addEventListener('DOMContentLoaded', checkAudioSupport);
