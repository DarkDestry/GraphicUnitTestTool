using System;
using System.Drawing;
using System.IO;

namespace GraphicUnitTestTool
{
	class Program
	{
		static void Main(string[] args)
		{
			//CHECK if args is correct
			if (args.Length < 3)
			{
				//Print help
				PrintHelp();
				return;
			}
			

			//CHECK if path is valid
			string img1path, img2path, outputpath;
			try
			{
				img1path = Path.GetFullPath(args[0]);
				img2path = Path.GetFullPath(args[1]);
				outputpath = Path.GetFullPath(args[2]);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

			//CHECK if image is same size
			Bitmap img1 = new Bitmap(img1path);
			Bitmap img2 = new Bitmap(img2path);
			if (img1.Height != img2.Height || img1.Width != img2.Width)
			{
				Console.WriteLine("Input images are of different width and height. Unable to compare");
			}


			Bitmap output = CompareAndOutput(img1, img2);

			output.Save(outputpath);

			string html = GenerateHTMLReport(img1path, img2path, outputpath);

			File.WriteAllText(outputpath+".html", html);
			
		}

		private static string GenerateHTMLReport(string img1path, string img2path, string outputpath)
		{
			string html = "<body>";

			html += $"<div style='width: 33.33%;display: inline-block;'><img src='{img1path}' style='width: 100%; height: auto'></div>";
			html += $"<div style='width: 33.33%;display: inline-block;'><img src='{outputpath}' style='width: 100%; height: auto'></div>";
			html += $"<div style='width: 33.33%;display: inline-block;'><img src='{img2path}' style='width: 100%; height: auto'></div>";

			html += "</body>";
			return html;
		}

		public static Bitmap CompareAndOutput(Bitmap img1, Bitmap img2)
		{
			Bitmap output = new Bitmap(img1.Width, img1.Height);

			for (int x = 0; x < img1.Width; x++)
			{
				for (int y = 0; y < img1.Height; y++)
				{
					if (img1.GetPixel(x,y) == img2.GetPixel(x,y))
					{
						Color pixel = img1.GetPixel(x, y);
						double h, s, v;
						ColorToHSV(pixel, out h, out s, out v);
						pixel = ColorFromHSV(h, s/10,  1-v/10);
						output.SetPixel(x, y, pixel);
					}
					else output.SetPixel(x,y,Color.Red);
				}
			}

			return output;
		}

		public static void PrintHelp()
		{
			string filename = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
			filename = Path.GetFileName(filename);
			Console.WriteLine("Graphic Unit Test Tool\n" +
			                  "This tool compares two images and highlights the changed pixels in an output file.\n");

			Console.WriteLine("Usage: " + filename + " pathToImg1 pathToImg2 outputPath [options]");
			Console.WriteLine("Options:");
			//Console.WriteLine("--tolerance=value | Sets a deltaE tolerance for flagging out pixel changes");
		}


		public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
		{
			int max = Math.Max(color.R, Math.Max(color.G, color.B));
			int min = Math.Min(color.R, Math.Min(color.G, color.B));

			hue = color.GetHue();
			saturation = (max == 0) ? 0 : 1d - (1d * min / max);
			value = max / 255d;
		}

		public static Color ColorFromHSV(double hue, double saturation, double value)
		{
			int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
			double f = hue / 60 - Math.Floor(hue / 60);

			value = value * 255;
			int v = Convert.ToInt32(value);
			int p = Convert.ToInt32(value * (1 - saturation));
			int q = Convert.ToInt32(value * (1 - f * saturation));
			int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

			if (hi == 0)
				return Color.FromArgb(255, v, t, p);
			else if (hi == 1)
				return Color.FromArgb(255, q, v, p);
			else if (hi == 2)
				return Color.FromArgb(255, p, v, t);
			else if (hi == 3)
				return Color.FromArgb(255, p, q, v);
			else if (hi == 4)
				return Color.FromArgb(255, t, p, v);
			else
				return Color.FromArgb(255, v, p, q);
		}
	}
}
