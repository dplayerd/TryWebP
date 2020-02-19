using ImageProcessor;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using NetFramework.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetFramework.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        { }

        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Index(HttpPostedFileBase image)
        {
            if (image == null)
                return View();

            if (image.ContentLength <= 0)
                return View();

            string[] allowImageTypes = new string[] { "image/jpeg", "image/png" };

            if (!allowImageTypes.Contains(image.ContentType.ToLower()))
                return View();


            string imagesPath = Path.Combine(Server.MapPath("~/"), "Images");
            string webPFileName = Path.GetFileNameWithoutExtension(image.FileName) + ".webp";

            string normalImagePath = Path.Combine(imagesPath, Path.GetFileName(image.FileName));
            string webPImagePath = Path.Combine(imagesPath, webPFileName);

            if (!Directory.Exists(imagesPath))
                Directory.CreateDirectory(imagesPath);


            //save image in original format
            using (var normalFileStream = new FileStream(normalImagePath, FileMode.Create))
            {
                image.InputStream.CopyTo(normalFileStream);
                image.InputStream.Position = 0;
            }

            //save image in WebP format
            using (var stream = new FileStream(webPImagePath, FileMode.Create))
            {
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                {
                    MemoryStream stream1 = new MemoryStream();
                    image.InputStream.CopyTo(stream1);
                    stream1.Flush();

                    

                    imageFactory.Load(stream1.ToArray())
                        .Format(new WebPFormat())
                        .Quality(85)
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