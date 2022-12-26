import tensorflow as tf

def convert_model(input_model_path, output_model_name):
    # Convert the model
    converter = tf.lite.TFLiteConverter.from_saved_model(input_model_path) # path to the SavedModel directory
    tflite_model = converter.convert()

    # Save the model.
    with open(output_model_name + '.tflite', 'wb') as f:
      f.write(tflite_model)


if __name__ == '__main__':
    convert_model('D:\JOSE\Documents\Proyectos C#\Pokedex\Pokedex.ModelBuilder\bin\Debug\net6.0\Workspace', 'D:\JOSE\Documents\Proyectos C#\Pokedex\Pokedex.ModelBuilder\bin\Debug\net6.0\Workspace\model')
    input()