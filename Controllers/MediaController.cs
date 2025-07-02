using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

[ApiController]
[Route("api/game")]
public class MediaController : ControllerBase {
    private const string UploadsFolder = "uploads";
    private readonly string FfmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "ffmpeg.exe");
    private static readonly string[] sourceArray = [".mp4", ".webm", ".ogv"];
    private static readonly string[] sourceArray0 =  [ ".mp3", ".ogg", ".wav" ];

    public MediaController() {
        if (!Directory.Exists(UploadsFolder)) Directory.CreateDirectory(UploadsFolder);
    }

    [HttpPost("video")]
    public IActionResult UploadVideo([FromForm] IFormFile videoFile) {
        if (videoFile == null || videoFile.Length == 0)
            return BadRequest("Video file cannot be empty");

        string cleanFileName = Path.GetFileNameWithoutExtension(videoFile.FileName).Replace(" ", "_");
        string originalFilePath = Path.Combine(UploadsFolder, cleanFileName + Path.GetExtension(videoFile.FileName));
        string outputMp4Path = Path.Combine(UploadsFolder, cleanFileName + "_converted.mp4");

        try {
            using (var stream = new FileStream(originalFilePath, FileMode.Create))
                videoFile.CopyTo(stream);

            if (!System.IO.File.Exists(FfmpegPath)) {
                return StatusCode(500, "FFmpeg is missing in the project. Ensure 'ffmpeg.exe' is in the 'ffmpeg' folder.");
            }

            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = FfmpegPath,
                    Arguments = $"-i \"{originalFilePath}\" -vf scale=640:360 -c:v libx264 -preset veryfast -crf 23 -c:a aac -b:a 128k -movflags +faststart \"{outputMp4Path}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string ffmpegOutput = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!System.IO.File.Exists(outputMp4Path)) {
                return StatusCode(500, "Error during MP4 conversion:\n" + ffmpegOutput);
            }

            System.IO.File.Delete(originalFilePath);

        } catch (Exception ex) { return StatusCode(500, "Error processing file: " + ex.Message); }

        return Ok(new { message = "Video uploaded and converted to .mp4", filePath = outputMp4Path });
    }


    [HttpGet("video/{fileName}")]
    public IActionResult GetVideo(string fileName) {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), UploadsFolder, fileName);

        Debug.WriteLine("Fichier trouvé sur le serveur : " + filePath);

        if (!System.IO.File.Exists(filePath)) {
            Debug.WriteLine("Fichier non trouvé sur le serveur : " + filePath);
            return NotFound("Fichier vidéo non trouvé");
        }

        var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        var mimeType = fileExtension switch {
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".ogv" => "video/ogg",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, mimeType, enableRangeProcessing: true);
    }

    [HttpGet("videos")]
    public IActionResult GetAllVideos() {
        var videoFiles = Directory.GetFiles(UploadsFolder)
                                .Where(file => sourceArray.Contains(Path.GetExtension(file).ToLowerInvariant()))
                                .Select(file => Path.GetFileName(file))
                                .ToList();

        return Ok(videoFiles);
    }

    [HttpGet("message")]
    public IActionResult GetMessage() => Ok(new { message = MessageManager.CurrentMessage });

    [HttpPost("image")]
    public IActionResult UploadImage([FromForm] IFormFile imageFile) {
        if (imageFile == null || imageFile.Length == 0) return BadRequest("Image file cannot be empty");

        string cleanFileName = imageFile.FileName.Replace(" ","");

        string filePath = Path.Combine(UploadsFolder, cleanFileName);

        try {
            using var stream = new FileStream(filePath, FileMode.Create);
            imageFile.CopyTo(stream);
        } catch (Exception ex) { return StatusCode(500, "Error processing file: " + ex.Message); }

        return Ok(new { message = "Image uploaded", filePath });
    }

    [HttpGet("image/{fileName}")]
    public IActionResult GetImage(string fileName) {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), UploadsFolder, fileName);

        if (!System.IO.File.Exists(filePath)) return NotFound("Image file not found");

        var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        var mimeType = fileExtension switch {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, mimeType);
    }

    [HttpGet("video/{fileName}/play")]
    public IActionResult PlayVideo(string fileName) {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), UploadsFolder, fileName);

        if (!System.IO.File.Exists(filePath)) return NotFound("Video file not found");

        var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        var mimeType = fileExtension switch {
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".ogv" => "video/ogg",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, mimeType, enableRangeProcessing: true);
    }

    [HttpDelete("image/{fileName}")]
    public IActionResult RemoveImage(string fileName) {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), UploadsFolder, fileName);

        if (!System.IO.File.Exists(filePath)) return NotFound("Image file not found");

        System.IO.File.Delete(filePath);
        return Ok(new { message = "Image removed" });
    }

    [HttpDelete("images")]
    public IActionResult RemoveAllImages() {
        var files = Directory.GetFiles(UploadsFolder);
        if (files.Length == 0) return NotFound("No image files to remove");

        foreach (var file in files) System.IO.File.Delete(file);
        return Ok(new { message = "All image files removed" });
    }

    [HttpGet("images")]
    public IActionResult GetAllImages() {
        var imageFiles = Directory.GetFiles(UploadsFolder)
                                .Where(file => new string[] { ".png", ".jpg", ".jpeg", ".gif" }.Contains(Path.GetExtension(file).ToLowerInvariant()))
                                .Select(file => Path.GetFileName(file))
                                .ToList();

        return Ok(imageFiles);
    }

    [HttpPost("audio")]
    public IActionResult UploadAudio([FromForm] IFormFile audioFile) {
        if (audioFile == null || audioFile.Length == 0) return BadRequest("Audio file cannot be empty");

        string cleanFileName = audioFile.FileName.Replace(" ","");

        string filePath = Path.Combine(UploadsFolder, cleanFileName);

        try {
            using var stream = new FileStream(filePath, FileMode.Create);
            audioFile.CopyTo(stream);
        } catch (Exception ex) { return StatusCode(500, "Error processing file: " + ex.Message); }

        return Ok(new { message = "Audio uploaded", filePath });
    }

    [HttpGet("audio/{fileName}")]
    public IActionResult GetAudio(string fileName) {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), UploadsFolder, fileName);

        if (!System.IO.File.Exists(filePath)) return NotFound("Audio file not found");

        var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        var mimeType = fileExtension switch {
            ".mp3" => "audio/mpeg",
            ".ogg" => "audio/ogg",
            ".wav" => "audio/wav",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, mimeType);
    }

    [HttpDelete("audio/{fileName}")]
    public IActionResult RemoveAudio(string fileName) {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), UploadsFolder, fileName);

        if (!System.IO.File.Exists(filePath)) return NotFound("Audio file not found");

        System.IO.File.Delete(filePath);
        return Ok(new { message = "Audio removed" });
    }

    [HttpDelete("audios")]
    public IActionResult RemoveAllAudios() {
        var files = Directory.GetFiles(UploadsFolder);
        if (files.Length == 0) return NotFound("No audio files to remove");

        foreach (var file in files) System.IO.File.Delete(file);
        return Ok(new { message = "All audio files removed" });
    }

    [HttpGet("audios")]
    public IActionResult GetAllAudios() {
        var audioFiles = Directory.GetFiles(UploadsFolder)
                                .Where(file => sourceArray0.Contains(Path.GetExtension(file).ToLowerInvariant()))
                                .Select(file => Path.GetFileName(file))
                                .ToList();

        return Ok(audioFiles);
    }

}
