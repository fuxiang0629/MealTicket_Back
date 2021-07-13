using FXCommon.FileUpload;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThoughtWorks.QRCode.Codec;

namespace FXCommon.Common
{
    public static class Utils
    {
        /// <summary>
        /// 检查手机号码是否正确
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <returns></returns>
        public static bool CheckPhone(this string phone)
        {
            if (string.IsNullOrEmpty(phone))//判断手机号为空
            {
                return false;
            }
            if (phone.Length != 11)
            {
                return false;
            }

            string regexPattern = "^((13[0-9])|(14[5,7])|(15[0-3,5-9])|(17[0,3,5-8])|(18[0-9])|166|198|199|(147))\\d{8}$";
            Regex regex = new Regex(regexPattern);
            if (!regex.IsMatch(phone))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str">加密字符串</param>
        /// <returns></returns>
        public static string ToMD5(this string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(output).Replace("-", "").ToLower();
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str">加密字符串</param>
        /// <returns></returns>
        public static string ToMD5(this byte[] str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(str);
            return BitConverter.ToString(output).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 生成随机的字符串
        /// </summary>
        /// <param name="codeCount">字符串长度</param>
        /// <returns></returns>
        public static string CreateRandomCode(int codeCount)
        {
            string allChar = "0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,a,b,c,d,e,f,g,h,i,g,k,l,m,n,o,p,q,r,F,G,H,I,G,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,s,t,u,v,w,x,y,z";
            string[] allCharArray = allChar.Split(',');
            string randomCode = "";
            int temp = -1;
            Random rand = new Random();
            for (int i = 0; i < codeCount; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * ((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(35);
                if (temp == t)
                {
                    return CreateRandomCode(codeCount);
                }
                temp = t;
                randomCode += allCharArray[t];
            }
            return randomCode;
        }

        /// <summary>
        /// 创建验证码图片
        /// </summary>
        /// <param name="validateCode"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CreateValidateGraphic(string validateCode)
        {
            string timeSpan = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            Bitmap image = new Bitmap(300, 150);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机生成器
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                //画图片的干扰线
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, x2, y1, y2);
                }
                Font font = new Font("Arial", 70, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateCode, font, brush, image.Width / 20, image.Height / 5);

                //画图片的前景干扰线
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);


                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] bytes = ms.GetBuffer();

                //保存图片数据
                string errorMessage = "";
                QiNiuHelper qiNiu = new QiNiuHelper();
                string path = qiNiu.Upload("png", bytes, "ValidateCode", out errorMessage);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    throw new Exception(errorMessage);
                }
                return path;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        public static byte[] CreateQrCode(string txtEncodeData)
        {
            //验证加密数据不能为空
            if (string.IsNullOrEmpty(txtEncodeData))
            {
                throw new Exception("二维码生成失败,内容不能为空");
            }
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            //编码方式
            //qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.NUMERIC;
            //验证图片大小参数(值越大生成的二维码图片像素越高)
            qrCodeEncoder.QRCodeScale = 4;

            //版本验证
            //设置为0主要是防止编码的字符串太长时发生错误
            qrCodeEncoder.QRCodeVersion = 7;
            //设置编码错误纠正级别
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;

            Bitmap image = qrCodeEncoder.Encode(txtEncodeData);

            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            return bytes;
        }

        /// <summary>
        /// 合并图片
        /// </summary>
        /// <param name="imgBack"></param>
        /// <param name="img"></param>
        /// <param name="xDeviation"></param>
        /// <param name="yDeviation"></param>
        /// <returns></returns>
        public static string CombinImage(string imgBackPath, string imgPath, int xRate = 0, int yRate = 0, int imgWidthRate = 0, int imgHeightRate = 0)
        {
            Image imgBack;
            System.Net.WebRequest webreq = System.Net.WebRequest.Create(imgBackPath);
            System.Net.WebResponse webres = webreq.GetResponse();
            using (System.IO.Stream stream = webres.GetResponseStream())
            {
                imgBack = Image.FromStream(stream);
            }
            Image img;
            System.Net.WebRequest webreq2 = System.Net.WebRequest.Create(imgPath);
            System.Net.WebResponse webres2 = webreq2.GetResponse();
            using (System.IO.Stream stream = webres2.GetResponseStream())
            {
                img = Image.FromStream(stream);
            }

            int imgWidth = imgBack.Width * imgWidthRate / 10000;
            int imgHeight = imgBack.Height * imgHeightRate / 10000;
            if (imgHeightRate == 0)
            {
                imgHeight = imgWidth;
            }
            if (imgWidthRate == 0)
            {
                imgWidth = imgHeight;
            }

            int x = imgBack.Width * xRate / 10000;
            int y = imgBack.Height * yRate / 10000;

            Bitmap bmp = new Bitmap(imgBack.Width, imgBack.Height);

            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.DrawImage(imgBack, 0, 0, imgBack.Width, imgBack.Height);

            g.DrawImage(img, x, y, imgWidth, imgHeight);
            GC.Collect();
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            bytes = ConvertToThumbnail(bytes, 80);


            //保存图片数据
            string errorMessage = "";
            QiNiuHelper qiNiu = new QiNiuHelper();
            string path = qiNiu.Upload("png", bytes, "Other.Img", out errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new Exception(errorMessage);
            }
            return path;
        }

        /// <summary>
        /// 图像缩略图处理
        /// </summary>
        /// <param name="bytes">图像源数据</param>
        /// <param name="compression">压缩质量 1-100</param>
        /// <param name="thumbWidth">缩略图的宽</param>
        /// <param name="thumbHeight">缩略图的高</param>
        /// <returns></returns>
        public static byte[] ConvertToThumbnail(byte[] bytes, int compression = 100, int thumbWidth = 0, int thumbHeight = 0)
        {
            byte[] bs = null;

            try
            {
                if (bytes != null)
                {
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        using (Bitmap srcimg = new Bitmap(ms))
                        {
                            if (thumbWidth == 0 && thumbHeight == 0)
                            {
                                thumbWidth = srcimg.Width;
                                thumbHeight = srcimg.Height;
                            }
                            using (Bitmap dstimg = new Bitmap(thumbWidth, thumbHeight))//图片压缩质量
                            {
                                //从Bitmap创建一个System.Drawing.Graphics对象，用来绘制高质量的缩小图。
                                using (Graphics gr = Graphics.FromImage(dstimg))
                                {
                                    //把原始图像绘制成上面所设置宽高的缩小图
                                    Rectangle rectDestination = new Rectangle(0, 0, thumbWidth, thumbHeight);
                                    gr.Clear(Color.WhiteSmoke);
                                    gr.CompositingQuality = CompositingQuality.HighQuality;
                                    gr.SmoothingMode = SmoothingMode.HighQuality;
                                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                    gr.DrawImage(srcimg, rectDestination, 0, 0, srcimg.Width, srcimg.Height, GraphicsUnit.Pixel);

                                    EncoderParameters ep = new EncoderParameters(1);
                                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compression);//设置压缩的比例1-100
                                    ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                                    ImageCodecInfo jpegICIinfo = arrayICI.FirstOrDefault(t => t.FormatID == System.Drawing.Imaging.ImageFormat.Png.Guid);
                                    using (MemoryStream dstms = new MemoryStream())
                                    {
                                        if (jpegICIinfo != null)
                                        {
                                            dstimg.Save(dstms, jpegICIinfo, ep);
                                        }
                                        else
                                        {
                                            dstimg.Save(dstms, System.Drawing.Imaging.ImageFormat.Png);//保存到内存里
                                        }
                                        bs = new Byte[dstms.Length];
                                        dstms.Position = 0;
                                        dstms.Read(bs, 0, bs.Length);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return bs;
        }

        /// <summary>
        /// 基于Sha1的自定义加密字符串方法：输入一个字符串，返回一个由40个字符组成的十六进制的哈希散列（字符串）。
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>加密后的十六进制的哈希散列（字符串）</returns>
        public static string Sha1(this string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA1.Create().ComputeHash(buffer);

            var sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }



        /// <summary>
        /// 压缩字节数组
        /// </summary>
        /// <param name="str"></param>
        public static byte[] Compress(byte[] inputBytes)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                using (GZipStream zipStream = new GZipStream(outStream, CompressionMode.Compress, true))
                {
                    zipStream.Write(inputBytes, 0, inputBytes.Length);
                    zipStream.Close(); //很重要，必须关闭，否则无法正确解压
                    return outStream.ToArray();
                }
            }
        }

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        /// <param name="str"></param>
        public static byte[] Decompress(byte[] inputBytes)
        {
            using (MemoryStream inputStream = new MemoryStream(inputBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        zipStream.CopyTo(outStream);
                        zipStream.Close();
                        return outStream.ToArray();
                    }
                }

            }
        }
        
        /// <summary>
        /// 压缩字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Compress(string input)
        {
            byte[] inputBytes = Encoding.Default.GetBytes(input);
            byte[] result = Compress(inputBytes);
            return Convert.ToBase64String(result);
        }
        /// <summary>
        /// 解压缩字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Decompress(string input)
        {
            byte[] inputBytes = Convert.FromBase64String(input);
            byte[] depressBytes = Decompress(inputBytes);
            return Encoding.Default.GetString(depressBytes);
        }

    }
}
