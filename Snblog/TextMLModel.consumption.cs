﻿using Microsoft.ML;
using Microsoft.ML.Data;

namespace Snblog;

/// <summary>
/// TextMLModel
/// </summary>
public partial class TextMLModel
{
    /// <summary>
    /// model input class for TextMLModel.
    /// </summary>
    #region model input class
    public class ModelInput
    {
        [ColumnName(@"title")]
        public string Title { get; set; }

        [ColumnName(@"describe")]
        public string Describe { get; set; }

        [ColumnName(@"url")]
        public string Url { get; set; }

    }

    #endregion

    /// <summary>
    /// model output class for TextMLModel.
    /// </summary>
    #region model output class
    public class ModelOutput
    {
        [ColumnName(@"title")]
        public float[] Title { get; set; }

        [ColumnName(@"describe")]
        public uint Describe { get; set; }

        [ColumnName(@"url")]
        public float[] Url { get; set; }

        [ColumnName(@"Features")]
        public float[] Features { get; set; }

        [ColumnName(@"PredictedLabel")]
        public string PredictedLabel { get; set; }

        [ColumnName(@"Score")]
        public float[] Score { get; set; }

    }

    #endregion

    private static string MLNetModelPath = Path.GetFullPath("TextMLModel.zip");

    public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

    /// <summary>
    /// Use this method to predict on <see cref="ModelInput"/>.
    /// </summary>
    /// <param name="input">model input.</param>
    /// <returns><seealso cref=" ModelOutput"/></returns>
    public static ModelOutput Predict(ModelInput input)
    {
        var predEngine = PredictEngine.Value;
        return predEngine.Predict(input);
    }
   

    private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
    {
        var mlContext = new MLContext();
        ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
        return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
    }
}