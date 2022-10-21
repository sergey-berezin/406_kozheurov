using kps7_1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

class Test
{
    static void print(Dictionary<string, float> result)
    {
        foreach (var item in result)
            Console.WriteLine($"{item.Key}: {item.Value}");
        Console.WriteLine("\n");
    }

    static void sync(Image<Rgb24> image)
    {
        var emotions = new emotion();
        print(emotions.Emotion(image));
    }
    static void async(Image<Rgb24> image1, Image<Rgb24> image2, Image<Rgb24> image3, Image<Rgb24> image4)
    {
        var emotions = new emotion();

        var CancellationTokens = new List<CancellationTokenSource>();
        CancellationTokens.Add(new CancellationTokenSource());
        CancellationTokens.Add(new CancellationTokenSource());
        CancellationTokens.Add(new CancellationTokenSource());
        CancellationTokens.Add(new CancellationTokenSource());

        var task1 = emotions.EmotionAsync(image1, CancellationTokens[0].Token);
        var task2 = emotions.EmotionAsync(image2, CancellationTokens[1].Token);
        var task3 = emotions.EmotionAsync(image3, CancellationTokens[2].Token);
        var task4 = emotions.EmotionAsync(image4, CancellationTokens[3].Token);


        var Task_List = new List<Task<Dictionary<string, float>>> { task1, task2, task3, task4 };
        while (Task_List.Count > 0)
        {
            var finished = Task.WaitAny(Task_List.ToArray());
            int num = -1;
            var result = Task_List[finished].Result;
            if (Task_List[finished] == task1)
                num = 0;
            else if (Task_List[finished] == task2)
                num = 1;
            else if (Task_List[finished] == task3)
                num = 2;
            else if (Task_List[finished] == task4)
                num = 3;
            Console.WriteLine("Test " + (num + 1).ToString() + " finished!\n");
            Task_List.RemoveAt(finished);
            print(result);

        }

    }

    static void async_canc(Image<Rgb24> image1, Image<Rgb24> image2, Image<Rgb24> image3, Image<Rgb24> image4)
    {
        var emotions = new emotion();
        var CancellationTokens = new List<CancellationTokenSource>();
        CancellationTokens.Add(new CancellationTokenSource());
        CancellationTokens.Add(new CancellationTokenSource());
        CancellationTokens.Add(new CancellationTokenSource());
        CancellationTokens.Add(new CancellationTokenSource());

        var task1 = emotions.EmotionAsync(image1, CancellationTokens[0].Token);
        var task2 = emotions.EmotionAsync(image2, CancellationTokens[1].Token);
        var task3 = emotions.EmotionAsync(image3, CancellationTokens[2].Token);
        var task4 = emotions.EmotionAsync(image4, CancellationTokens[3].Token);

        var Task_List = new List<Task<Dictionary<string, float>>> { task1, task2, task3, task4 };

        try
        {
            CancellationTokens[0].Cancel();
        }
        catch (AggregateException ae)
        {
            Console.WriteLine("Task 1 successfully cancelled!\n");
        }


        try
        {
            CancellationTokens[2].Cancel();
        }
        catch (AggregateException ae)
        {
            Console.WriteLine("Task 3 successfully cancelled!\n");
        }

        while (Task_List.Count > 0)
        {
            try
            {
                var finished = Task.WaitAny(Task_List.ToArray());
                int num = -1;
                var result = Task_List[finished].Result;
                if (Task_List[finished] == task1)
                    num = 0;
                else if (Task_List[finished] == task2)
                    num = 1;
                else if (Task_List[finished] == task3)
                    num = 2;
                else if (Task_List[finished] == task4)
                    num = 3;
                Console.WriteLine("Test " + (num + 1).ToString() + " finished!\n");
                Task_List.RemoveAt(finished);
                print(result);

            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.InnerExceptions)
                {
                    if (e is TaskCanceledException)
                    {
                        TaskCanceledException ex = (TaskCanceledException)e;
                        Console.WriteLine(ex.Message);
                        Task_List.Remove(Task_List.Find(x => x.Id.Equals(ex.Task.Id)));
                    }
                    else
                        Console.WriteLine(e.Message);
                }
            }
        }



    }
    static void Main()
    {
        using Image<Rgb24> image1 = Image.Load<Rgb24>("face1.png");
        using Image<Rgb24> image2 = Image.Load<Rgb24>("face2.png");
        using Image<Rgb24> image3 = Image.Load<Rgb24>("face3.jpg");
        using Image<Rgb24> image4 = Image.Load<Rgb24>("face4.jpg");

        Console.WriteLine("Синхронный тест\n");
        sync(image1);
        Console.WriteLine("Асинхронный тест\n");
        async(image1, image2, image3, image4);
        Console.WriteLine("Асинхронный тест с отменой\n");
        async_canc(image1, image2, image3, image4);
    }

};

