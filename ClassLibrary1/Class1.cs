using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ClassLibrary1
{
    public class Class1
    {
        private SemaphoreSlim CancellationTokensSemaphore;
        private InferenceSession Session;
        private int numb;
        private IDisposableReadOnlyCollection<DisposableNamedOnnxValue>[] results;

        public Class1()
        {
            this.Session = new InferenceSession("emotion-ferplus-7.onnx");
            this.CancellationTokensSemaphore = new SemaphoreSlim(1);
        }

        static DenseTensor<float> GrayscaleImageToTensor(Image<Rgb24> img)
        {
            var w = img.Width;
            var h = img.Height;
            var t = new DenseTensor<float>(new[] { 1, 1, h, w });

            img.ProcessPixelRows(pa =>
            {
                for (int y = 0; y < h; y++)
                {
                    Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        t[0, 0, y, x] = pixelSpan[x].R; // B and G are the same
                    }
                }
            });

            return t;
        }

        static float[] Softmax(float[] z)
        {
            var exps = z.Select(x => Math.Exp(x)).ToArray();
            var sum = exps.Sum();
            return exps.Select(x => (float)(x / sum)).ToArray();
        }

        private Dictionary<string, float> result(float[] res)
        {
            string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
            var result = new Dictionary<string, float>();
            for (int i = 0; i < keys.Length; i++)
                result[keys[i]] = res[i];
            return result;
        }

        public Dictionary<string, float> Emotion(Image<Rgb24> image )
        {
            image.Mutate(ctx => {
                ctx.Resize(new Size(64, 64));
                // ctx.Grayscale();
            });

            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = Session.Run(inputs);
            return result(Softmax(results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray()));
        }



        public async Task<Dictionary<string, float>> EmotionAsync(Image<Rgb24> image)
        {
            var cancellation_token_source = new CancellationTokenSource();
            var res = await Task<Dictionary<string, float>>.Run(async() =>
            {
                image.Mutate(ctx =>
                {
                    ctx.Resize(new Size(64, 64));
                    // ctx.Grayscale();
                });

                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };
                await CancellationTokensSemaphore.WaitAsync();
                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = Session.Run(inputs);
                CancellationTokensSemaphore.Release();
                return result(Softmax(results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray()));
            });
            return res;
        }


    }



}