using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public partial class TextToSpeech : MonoBehaviour
{
    private AudioSource audioSource;
    private TTSInference inference;

    private int sampleLength;
    private float[] _audioSample;

    private AudioClip _audioClip;
    private bool _playAudio = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        inference = GetComponent<TTSInference>();
    }

    void Update()
    {
        if (_playAudio)
        {
            _audioClip = AudioClip.Create("Speak", sampleLength, 1, 22050, false);
            _audioClip.SetData(_audioSample, 0);
            audioSource.PlayOneShot(_audioClip);
            _playAudio = false;
        }
    }

    public void Speak(string text)
    {
        inference.InitTTSInference();
        SpeakTask(text);
        inference.Dispose();
    }

    private void SpeakTask(object inputText)
    {
        string text = inputText as string;
        int[] inputIDs = TTSProcessor.TextToSequence(ref text);
        float[,,] fastspeechOutput = inference.FastspeechInference(ref inputIDs);
        float[,,] melganOutput = inference.MelganInference(ref fastspeechOutput);

        sampleLength = melganOutput.GetLength(1);
        _audioSample = new float[sampleLength];
        for (int s = 0; s < sampleLength; s++) _audioSample[s] = melganOutput[0, s, 0];
        _playAudio = true;
    }
}