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

			//CHECK for OPTIONS 
			float tolerance = 0;
			foreach (var s in args)
			{
				if (s.Contains("--tolerance="))
				{
					if (!float.TryParse(s.Substring(12), out tolerance))
					{
						Console.WriteLine("Invalid tolerance value.");
						return;
					}
				}
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
				Console.WriteLine("Invalid Image Paths provided");
				throw;
			}

			//CHECK if image is same size
			Bitmap img1 = new Bitmap(img1path);
			Bitmap img2 = new Bitmap(img2path);
			if (img1.Height != img2.Height || img1.Width != img2.Width)
			{
				Console.WriteLine("Input images are of different width and height. Unable to compare");
				return;
			}



			//Execute Comparison
			ComparisonReport report = CompareImages(img1, img2, tolerance);
			string html = GenerateHTMLReport(img1path, img2path, outputpath, report);

			//Dump comparison output
			report.output.Save(outputpath);
			File.WriteAllText(outputpath+".html", html);
		}

		private static string GenerateHTMLReport(string img1path, string img2path, string outputpath, ComparisonReport report)
		{
			string html = "<body>";

			html += $"<div style='width: 33.33%;display: inline-block;'><img src='{img1path}' style='width: 100%; height: auto'></div>";
			html += $"<div style='width: 33.33%;display: inline-block;'><img src='{outputpath}' style='width: 100%; height: auto'></div>";
			html += $"<div style='width: 33.33%;display: inline-block;'><img src='{img2path}' style='width: 100%; height: auto'></div>";
			html += $"<div>" +
			        $"<p>&Delta;E is the perceivable difference in color. A &Delta;E of 0 is mathematically exact. A &Delta;E of 1.0 to 2.0 is just noticeable.</p>" +
			        $"<table style='width:20em'>" +
			        $"<tr>" +
			        $"<td>Maximum &Delta;E</td>" +
			        $"<td>{report.maxDeltaE}</td>" +
			        $"</tr>" +
			        $"<tr>" +
			        $"<td>Minimum &Delta;E</td>" +
			        $"<td>{report.minDeltaE}</td>" +
			        $"</tr>" +
			        $"<tr>" +
			        $"<td>Average &Delta;E</td>" +
			        $"<td>{report.avgDeltaE}</td>" +
			        $"</tr>" +
			        $"</table></div>";

			html += "</body>";
			return html;
		}

		public static ComparisonReport CompareImages(Bitmap img1, Bitmap img2, float tolerance = 0)
		{
			Bitmap output = new Bitmap(img1.Width, img1.Height);
			float maxDeltaE = float.MinValue;
			float minDeltaE = float.MaxValue;
			float totalDeltaE = 0;
			float deltaEValues = 0;
			float avgDeltaE = 0;

			for (int x = 0; x < img1.Width; x++)
			{
				for (int y = 0; y < img1.Height; y++)
				{
					Color pixel1 = img1.GetPixel(x,y);
					Color pixel2 = img2.GetPixel(x,y);
					ColorFormulas cf1 = new ColorFormulas(pixel1.R, pixel1.G, pixel1.B);
					ColorFormulas cf2 = new ColorFormulas(pixel2.R, pixel2.G, pixel2.B);
					float deltaE = (cf1.CompareTo(cf2));

					if (minDeltaE > deltaE) minDeltaE = deltaE;
					if (maxDeltaE < deltaE) maxDeltaE = deltaE;
					totalDeltaE += deltaE;
					deltaEValues++;

					if (deltaE <= tolerance)
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

			avgDeltaE = totalDeltaE / deltaEValues;

			return new ComparisonReport(output,maxDeltaE,minDeltaE,avgDeltaE);
		}

		public static void PrintHelp()
		{
			string filename = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
			filename = Path.GetFileName(filename);
			Console.WriteLine("");
			Console.WriteLine("===== Graphic Unit Test Tool =====\n" +
			                  "This tool compares two images and highlights the changed pixels in an output file.\n");

			Console.WriteLine("Usage: " + filename + " pathToImg1 pathToImg2 outputPath [options]");
			Console.WriteLine("Example: " + filename + " .\\img1.png .\\img2.png .\\img3.png --tolerance=1.5");
			Console.WriteLine("");
			Console.WriteLine("===== Arguments =====");
			Console.WriteLine("pathToImg1        | Left side image path");
			Console.WriteLine("pathToImg2        | Right side image path");
			Console.WriteLine("outputPath        | Image path to output the highlighted difference");
			Console.WriteLine("");
			Console.WriteLine("===== Options =====");
			Console.WriteLine("--tolerance=value | Sets a deltaE tolerance for highlighting out pixel changes");
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

		public class ComparisonReport
		{
			public Bitmap output;
			public float maxDeltaE;
			public float minDeltaE;
			public float avgDeltaE;

			public ComparisonReport(Bitmap output, float maxDeltaE, float minDeltaE, float avgDeltaE)
			{
				this.output = output;
				this.maxDeltaE = maxDeltaE;
				this.minDeltaE = minDeltaE;
				this.avgDeltaE = avgDeltaE;
			}
		}
	}
}
