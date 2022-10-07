using ClassLibrary1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

class Test
{
    static string GenreateTokenKey()
    { return Guid.NewGuid().ToString(); }
    static void print(Dictionary<string, float> result)
    {
        foreach (var item in result)
            Console.WriteLine($"{item.Key}: {item.Value}");
        Console.WriteLine("\n");
    }

    static void sync (Image<Rgb24> image)
    {
        var emotions = new Class1();
        print(emotions.Emotion(image));
    }
    static void async(Image<Rgb24> image1, Image<Rgb24> image2, Image<Rgb24> image3, Image<Rgb24> image4)
    {
        var emotions = new Class1();
        var task1 = emotions.EmotionAsync(image1, GenreateTokenKey());
        var task2 = emotions.EmotionAsync(image2, GenreateTokenKey());
        var task3 = emotions.EmotionAsync(image3, GenreateTokenKey());
        var task4 = emotions.EmotionAsync(image4, GenreateTokenKey());

        
        var Task_List = new List<Task<Dictionary<string, float>>> { task1, task2, task3, task4};
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
            Console.WriteLine( "Test " +(num+1).ToString() + " finished!\n");
            Task_List.RemoveAt(finished);
            print(result);

        }

    }

    static void async_canc(Image<Rgb24> image1, Image<Rgb24> image2, Image<Rgb24> image3, Image<Rgb24> image4)
    {
        var emotions = new Class1();
        string same_faces_test_token1 = GenreateTokenKey();
        string same_faces_test_token2 = GenreateTokenKey();

        var task1 = emotions.EmotionAsync(image1, same_faces_test_token1);
        var task2 = emotions.EmotionAsync(image2, GenreateTokenKey());
        var task3 = emotions.EmotionAsync(image3, same_faces_test_token2);
        var task4 = emotions.EmotionAsync(image4, GenreateTokenKey());

        if (emotions.Cancel(same_faces_test_token1))
        {
            Console.WriteLine("Task 1 successfully cancelled!\n");
        }
        if (emotions.Cancel(same_faces_test_token2))
        {
            Console.WriteLine("Task 3 successfully cancelled!\n");
        }

        var Task_List = new List<Task<Dictionary<string, float>>> { task1, task2, task3, task4 };
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
        Console.WriteLine("Асинхронный тест с отменой\n");
        async_canc(image1, image2, image3, image4);
        Console.WriteLine("Асинхронный тест\n");
        async(image1, image2, image3, image4);
        Console.WriteLine("Синхронный тест\n");
        sync(image1);

    }

};

