using System;
using System.IO;
using UnityEngine;
using TensorFlowLite;

public class TTSInference : MonoBehaviour, IDisposable
{
    [SerializeField]
    private string fastspeech;
    [SerializeField]
    private string melgan;
    [Range(0.0f, 1.0f), Tooltip("Lower Value = Faster Speed")]
    public float speedRatio = 1.0f;

    private int speakerID = 1;

    private Interpreter _fastspeechInterpreter;
    private Interpreter _melganInterpreter;
    private InterpreterOptions _options;

    public void InitTTSInference()
    {
        _options = new InterpreterOptions() { threads = 4 };
        _fastspeechInterpreter = new Interpreter(ReadStreamingAssetFileByte(fastspeech + ".tflite"), _options);
        _melganInterpreter = new Interpreter(ReadStreamingAssetFileByte(melgan + ".tflite"), _options);
    }

    #region Inferencing
    private Array[] PrepareInput(ref int[] inputIDs, ref int speakerID, ref float speedRatio)
    {
        Array[] inputData = new Array[3];

        int[,] formatedInputIDS = new int[1, inputIDs.Length];
        for (int i = 0; i < inputIDs.Length; i++) formatedInputIDS[0, i] = inputIDs[i];
        speedRatio = Mathf.Clamp(speedRatio, 0.0f, 1.0f);
        inputData[0] = formatedInputIDS;
        inputData[1] = new int[1] { speakerID };
        inputData[2] = new float[1] { speedRatio };

        return inputData;
    }

    public float[,,] FastspeechInference(ref int[] inputIDs)
    {
        _fastspeechInterpreter.ResizeInputTensor(0, new int[2] { 1, inputIDs.Length });
        _fastspeechInterpreter.ResizeInputTensor(1, new int[1] { 1 });
        _fastspeechInterpreter.ResizeInputTensor(2, new int[1] { 1 });

        _fastspeechInterpreter.AllocateTensors();
        Array[] inputData = PrepareInput(ref inputIDs, ref speakerID, ref speedRatio);
        for (int d = 0; d < inputData.Length; d++)
            _fastspeechInterpreter.SetInputTensorData(d, inputData[d]);

        _fastspeechInterpreter.Invoke();

        int[] outputShape = _fastspeechInterpreter.GetOutputTensorInfo(1).shape;
        float[,,] outputData = new float[outputShape[0], outputShape[1], outputShape[2]];
        _fastspeechInterpreter.GetOutputTensorData(1, outputData);
        return outputData;
    }

    public float[,,] MelganInference(ref float[,,] spectogram)
    {
        _melganInterpreter.ResizeInputTensor(0, new int[3]{
        spectogram.GetLength(0),
        spectogram.GetLength(1),
        spectogram.GetLength(2)});

        _melganInterpreter.AllocateTensors();
        _melganInterpreter.SetInputTensorData(0, spectogram);

        _melganInterpreter.Invoke();

        int[] outputShape = _melganInterpreter.GetOutputTensorInfo(0).shape;
        float[,,] outputData = new float[outputShape[0], outputShape[1], outputShape[2]];
        _melganInterpreter.GetOutputTensorData(0, outputData);
        return outputData;
    }
    #endregion

    public void Dispose()
    {
        _fastspeechInterpreter?.Dispose();
        _melganInterpreter?.Dispose();
        _options?.Dispose();
    }

    public static byte[] ReadStreamingAssetFileByte(string path) => File.ReadAllBytes(GetStreamingAssetFilePath(path));
    public static string GetStreamingAssetFilePath(string streamingAssetPath) => Path.Combine(Application.streamingAssetsPath, streamingAssetPath);
}