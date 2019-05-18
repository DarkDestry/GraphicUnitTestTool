# GraphicUnitTestTool
This tool compares two images and highlights the output in a separate image

```
-----------------------------------------------------------------------------------
===== Graphic Unit Test Tool =====
This tool compares two images and highlights the changed pixels in an output file.

Usage: GraphicUnitTestTool.exe pathToImg1 pathToImg2 outputPath [options]
Example: GraphicUnitTestTool.exe .\img1.png .\img2.png .\img3.png --tolerance=1.5

===== Arguments =====
pathToImg1        | Left side image path
pathToImg2        | Right side image path
outputPath        | Image path to output the highlighted difference

===== Options =====
--tolerance=value | Sets a deltaE tolerance for highlighting out pixel changes
-----------------------------------------------------------------------------------
```

## HTML output
![output image](https://i.imgur.com/I8oxN5T.png)
