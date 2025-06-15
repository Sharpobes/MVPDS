namespace MVPDS.Services;
using Microsoft.JSInterop;
public static class MediaRecorderInterop
{
    public static ValueTask StartRecordingAsync<T>(IJSRuntime js, DotNetObjectReference<T> dotNetRef) where T : class
        => js.InvokeVoidAsync("MediaRecorderInterop.startRecording", dotNetRef);


    public static ValueTask StopRecordingAsync(IJSRuntime js)
        => js.InvokeVoidAsync("MediaRecorderInterop.stopRecording");

    public static ValueTask PlayAudioAsync(IJSRuntime js, byte[] audioData)
        => js.InvokeVoidAsync("MediaRecorderInterop.playAudio", audioData);
}