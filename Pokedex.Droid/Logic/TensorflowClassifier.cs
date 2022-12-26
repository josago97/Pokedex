﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Java.IO;
using Java.Nio;
using Java.Nio.Channels;
using Xamarin.TensorFlow.Lite;

namespace Pokedex.Droid.Logic
{
    public class ImageClassificationModel
    {
        public ImageClassificationModel(string tagName, float probability)
        {
            TagName = tagName;
            Probability = probability;
        }

        public float Probability { get; }
        public string TagName { get; }
    }

    // Class prepared for when the model can be used with Tensorflow Lite and Tensorflow.Lite-Select-TF-Ops
    public class TensorflowClassifier
    {
        private const string MODEL_NAME = "model.tflite";

        private Interpreter _interpreter;

        public TensorflowClassifier()
        {
            _interpreter = GetInterpreter();
        }

        private Interpreter GetInterpreter()
        {
            //Convert model.tflite to Java.Nio.MappedByteBuffer, the require type for Xamarin.TensorFlow.Lite.Interpreter
            AssetFileDescriptor assetDescriptor = Application.Context.Assets.OpenFd(MODEL_NAME);
            FileInputStream inputStream = new FileInputStream(assetDescriptor.FileDescriptor);
            MappedByteBuffer mappedByteBuffer = inputStream.Channel.Map(FileChannel.MapMode.ReadOnly, assetDescriptor.StartOffset, assetDescriptor.DeclaredLength);

            return new Interpreter(mappedByteBuffer);
        }



        //FloatSize is a constant with the value of 4 because a float value is 4 bytes
        const int FloatSize = 4;
        //PixelSize is a constant with the value of 3 because a pixel has three color channels: Red Green and Blue
        const int PixelSize = 3;

        public List<ImageClassificationModel> Classify(byte[] image)
        {
            //To resize the image, we first need to get its required width and height
            var tensor = _interpreter.GetInputTensor(0);
            var shape = tensor.Shape();

            var width = shape[1];
            var height = shape[2];

            var byteBuffer = GetPhotoAsByteBuffer(image, width, height);

            //use StreamReader to import the labels from labels.txt
            var streamReader = new StreamReader(Application.Context.Assets.Open("labels.txt"));

            //Transform labels.txt into List<string>
            var labels = streamReader.ReadToEnd().Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

            //Convert our two-dimensional array into a Java.Lang.Object, the required input for Xamarin.TensorFlow.List.Interpreter
            var outputLocations = new float[1][] { new float[labels.Count] };
            var outputs = Java.Lang.Object.FromArray(outputLocations);

            _interpreter.Run(byteBuffer, outputs);
            var classificationResult = outputs.ToArray<float[]>();

            //Map the classificationResult to the labels and sort the result to find which label has the highest probability
            var classificationModelList = new List<ImageClassificationModel>();

            for (var i = 0; i < labels.Count; i++)
            {
                var label = labels[i]; 
                classificationModelList.Add(new ImageClassificationModel(label, classificationResult[0][i]));
            }

            return classificationModelList;
        }


        //Resize the image for the TensorFlow interpreter
        private ByteBuffer GetPhotoAsByteBuffer(byte[] image, int width, int height)
        {
            var bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);
            var resizedBitmap = Bitmap.CreateScaledBitmap(bitmap, width, height, true);

            var modelInputSize = FloatSize * height * width * PixelSize;
            var byteBuffer = ByteBuffer.AllocateDirect(modelInputSize);
            byteBuffer.Order(ByteOrder.NativeOrder());

            var pixels = new int[width * height];
            resizedBitmap.GetPixels(pixels, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

            var pixel = 0;

            //Loop through each pixels to create a Java.Nio.ByteBuffer
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var pixelVal = pixels[pixel++];

                    byteBuffer.PutFloat(pixelVal >> 16 & 0xFF);
                    byteBuffer.PutFloat(pixelVal >> 8 & 0xFF);
                    byteBuffer.PutFloat(pixelVal & 0xFF);
                }
            }

            bitmap.Recycle();

            return byteBuffer;
        }
    }
}