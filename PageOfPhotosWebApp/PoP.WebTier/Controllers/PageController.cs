﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MediaRepository;
using PoP.Models;
using ValetKeyPattern.AzureStorage;

namespace PoP.WebTier.Controllers
{
    public class PageController : Controller
    {
        //
        // GET: /Page/

        public ActionResult Index(string slug)
        {
            if (String.IsNullOrEmpty(slug))
                return RedirectToAction("Index", "Home");

            var pageModel = GetFakePageModel(slug);

            return View(pageModel);
        }

#if false
        public ActionResult Index()
        {
            return View();
        }
#endif

      private PageModel GetFakePageModel(string slug)
      {
         var m1 = new ImageRepresentation
         {
            DisplaySize = new Size(14, 20),
            MimeType = "image/jpeg",
            Url = "https://pop.blob.core.windows.net/photos/perfect_beer.jpg",
            Title = "Mmmmm... Beer"
         };
         var m2 = new ImageRepresentation
         {
            MimeType = "image/jpeg",
            Url = "https://pop.blob.core.windows.net/photos/perfect_beer.jpg",
            Title = "Dublin Beer"
         };
         var m3 = new ImageRepresentation
         {
            DisplaySize = new Size(34, 30),
            MimeType = "image/jpeg",
            Url = "https://pop.blob.core.windows.net/photos/perfect_beer.jpg",
            Title = "Da Beer"
         };
         var mediaRepresentationList = new List<MediaRepresentation> { m1, m2, m3 };
         var fakePageModel = new PageModel
         {
            Name = slug,
            Description = "Pages for " + slug,
            Media = mediaRepresentationList
         };

         return fakePageModel;
      }

// TODO: re-enable      [Authorize]
      [HttpGet]
      public ActionResult Upload(string msg)
      {
         ViewBag.Message = ""; // make sure there's always a valid string to simplify error checking in the view
         if (!String.IsNullOrEmpty(msg)) ViewBag.Message = msg;
         return View();
      }

  // TODO: re-enable    [Authorize]
      [HttpPost]
      public ActionResult Upload(IEnumerable<HttpPostedFileBase> files)
      {
         var fileList = String.Empty;
         var firstFile = true;

         foreach (var file in files.Where(file => file != null && file.ContentLength > 0))
         {
            Contract.Assert(file.FileName == Path.GetFileName(file.FileName)); // browsers should not send path info - but synthetic test could

            var fileExtension = Path.GetExtension(file.FileName);
            if (!String.IsNullOrWhiteSpace(fileExtension)) fileExtension = fileExtension.ToLower();
            var destinationUrl = String.Format(ConfigurationManager.AppSettings["MediaStorageUrlFile.ExtTemplate"], Guid.NewGuid(), fileExtension);
            var blobValetKeyUrl = ConfigurationManager.AppSettings["MediaStorageValetKeyUrl"];
            var queueValetKeyUrl = ConfigurationManager.AppSettings["MediaIngestionQueueValetKeyUrl"];

            var blobValet = new BlobValet(blobValetKeyUrl);
            var queueValet = new QueueValet(queueValetKeyUrl);

            MediaIngester.CaptureUploadedMedia(blobValet, queueValet, file.InputStream, file.FileName, file.ContentType, file.ContentLength, destinationUrl);

            if (!firstFile) fileList += ", ";
            fileList += file.FileName;
            firstFile = false;
         }
         if (String.IsNullOrWhiteSpace(fileList))
         {
            return RedirectToAction("Upload");
         }
         else
         {
            var message = String.Format("Upload successful for: {0}", fileList);
            return RedirectToAction("Upload", new { msg = message });
         }
      }
   }
}
