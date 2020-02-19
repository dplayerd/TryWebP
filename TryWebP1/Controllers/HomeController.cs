using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageProcessor;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TryWebP1.Models;

namespace TryWebP1.Controllers
{
    public class HomeController : Controller
    {
        IHostingEnvironment hostingEnvironment;

        public HomeController(IHostingEnvironment environment)
        {
            this.hostingEnvironment = environment;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormFile image)
        {
            if (image == null)
                return View();

            if (image.Length < 0)
                return View();

            string[] allowImageTypes = new string[] { "image/jpeg", "image/png" };

            if (!allowImageTypes.Contains(image.ContentType.ToLower()))
                return View();


            string imagesPath = Path.Combine(hostingEnvironment.WebRootPath, "Images");
            string webPFileName = Path.GetFileNameWithoutExtension(image.FileName) + ".webp";

            string normalImagePath = Path.Combine(imagesPath, image.FileName);
            string webPImagePath = Path.Combine(imagesPath, webPFileName);

            if (!Directory.Exists(imagesPath))
                Directory.CreateDirectory(imagesPath);


            //save image in original format
            using (var normalFileStream = new FileStream(normalImagePath, FileMode.Create))
            {
                image.CopyTo(normalFileStream);
            }

            //save image in WebP format
            using (var stream = new FileStream(webPImagePath, FileMode.Create))
            {
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                {
                    imageFactory.Load(image.OpenReadStream())
                        .Format(new WebPFormat())
                        .Quality(100)
                        .Save(stream);
                }
            }


            Images viewModel = new Images();
            viewModel.NormalImage = "/Images/" + image.FileName;
            viewModel.WebPImage = "/Images/" + webPFileName;

            return View(viewModel);
        }
    }
}
