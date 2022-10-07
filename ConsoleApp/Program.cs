using ClassLibrary1;
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

    static void sync (Image<Rgb24> image)
    {
        var emotions = new Class1();
        print(emotions.Emotion(image));
    }
    static async void async(Image<Rgb24> image1, Image<Rgb24> image2, Image<Rgb24> image3, Image<Rgb24> image4)
    {
        var emotions = new Class1();
        var task1 = emotions.EmotionAsync(image1);
        var task2 = emotions.EmotionAsync(image2);
        var task3 = emotions.EmotionAsync(image3);
        var task4 = emotions.EmotionAsync(image4);

        
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
        
    }

};

