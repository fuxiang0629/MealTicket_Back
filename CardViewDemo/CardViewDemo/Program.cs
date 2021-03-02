using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Tesseract;

namespace CardViewDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] b = File.ReadAllBytes("C:\\Users\\fuxiang\\Desktop/1.png");
            var temp = Convert.ToBase64String(b);
            string text = HttpUtility.UrlEncode(temp, System.Text.Encoding.UTF8);
            //string temp = ImageToText("C:\\Users\\fuxiang\\Desktop/1.png");
            Console.WriteLine(temp);
            Console.ReadLine();
        }

        //调用tesseract实现OCR识别
        public static string ImageToText(string imgPath)
        {
            using (var engine = new TesseractEngine("tessdata", "chi_sim", EngineMode.TesseractOnly))
            {
                using (var img = Pix.LoadFromFile(imgPath))
                {
                    using (var page = engine.Process(img))
                    {
                        return page.GetText();
                    }
                }
            }
        }
    }
}
