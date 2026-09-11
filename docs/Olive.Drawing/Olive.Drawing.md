**Olive.Drawing**

### Overview
The `Olive.Drawing` namespace provides a set of utilities for image processing, including optimization, resizing, rotation, compression, and color manipulation. These utilities leverage `SkiaSharp` and `System.Drawing` to offer efficient and high-quality image transformations.

---

## **ImageOptimizer**

### **Description**
The `ImageOptimizer` class is used to resize and optimize images while maintaining a balance between quality and file size.

### **Properties**
- **MaximumWidth** (int): The maximum allowed width for the optimized image (default is 900).
- **MaximumHeight** (int): The maximum allowed height for the optimized image (default is 700).
- **Quality** (int): The quality setting for the output image (default is 80).
- **OutputFormat** (enum): The format of the output image (JPEG, PNG, BMP, GIF).

### **Methods**
- **Optimize(SKBitmap source)**: Resizes and optimizes the input image while maintaining the aspect ratio.
- **Optimize(byte[] sourceData, bool toJpeg = true)**: Optimizes the given image byte array and returns a new byte array.
- **Optimize(string sourceImagePath, string optimizedImagePath)**: Reads an image from a file, optimizes it, and saves the result to a new file.

---

## **GraphicExtensions**

### **Description**
This static class contains helper methods for working with `Bitmap` images, including column extraction, rotation, and insertion of images.

### **Methods**
- **GetColumn(Bitmap image, int columnIndex)**: Extracts a single-column image from the specified bitmap.
- **GetWidth(Font font, string text, bool useAntialias)**: Calculates the width of a given text using the specified font.
- **Rotate(Image image, double degrees)**: Rotates an image clockwise by the specified number of degrees.
- **Stretch(Bitmap image, int width)**: Stretches a single-column image to a new width.
- **SaveAsGif(Bitmap image, string path, bool transparent)**: Saves an image as a GIF with optional transparency.

---

## **GifPalleteGenerator**

### **Description**
This class generates a color palette for GIF images based on the colors used in the image.

### **Methods**
- **GeneratePallete(Bitmap image)**: Creates a color palette optimized for the given image.
- **FindAllColours(Bitmap image)**: Identifies and sorts all unique colors in an image by frequency.

---

## **BitmapHelper**

### **Description**
Provides various helper functions for working with bitmap images, including resizing, brightness adjustment, and cropping.

### **Methods**
- **ToBuffer(Image image, ImageFormat format, int quality = 100)**: Converts an image to a byte array with the specified format and quality.
- **FromBuffer(byte[] buffer)**: Converts a byte array to an image.
- **IsValidImage(byte[] buffer)**: Checks if the given byte array contains a valid image.
- **Resize(Image source, int newWidth, int newHeight)**: Resizes an image to the specified dimensions.
- **Brighten(Image source, int level)**: Adjusts the brightness of an image.
- **Crop(Image image, Rectangle rectangle)**: Crops an image to the specified rectangle.

---

## **Extensions (Blob Optimization)**

### **Description**
These methods provide image optimization capabilities for `Blob` objects, commonly used in storage and database applications.

### **Methods**
- **OptimizeImage(Blob blob, int maxWidth, int maxHeight, int quality, bool toJpeg = true)**: Optimizes an image stored as a `Blob`.
- **SetData(byte[] data)**: Updates the `Blob` with optimized image data.

---

## **Use Cases**
- **Web Applications:** Optimize images before serving them to users to reduce bandwidth usage.
- **Database Storage:** Store images efficiently with controlled dimensions and quality.
- **Image Processing Pipelines:** Apply transformations such as cropping, resizing, and brightness adjustments before analysis.
- **GIF Optimization:** Generate optimized GIFs with reduced color palettes to save space.

---

This documentation provides an overview of the key functionalities in `Olive.Drawing`. For further details, refer to the implementation or usage examples in the source code.

