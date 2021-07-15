using FilmFree.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FilmFree.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        // GET: AdminMail_Sender mail_Sender = new Mail_Sender();
        FilmFreeEntities DB = new FilmFreeEntities();
        public Kullanicilar KullaniciOturumAcmis()
        {
            string kAdi = "", Dogrulama = "";
            try
            {
                kAdi = HttpContext.User.Identity.Name.Split(',')[0]; Dogrulama = HttpContext.User.Identity.Name.Split(',')[2];
            }
            catch
            {
                return null;
            }
            return DB.Kullanicilar.FirstOrDefault(x => x.kAdi == kAdi && x.Dogrulama == Dogrulama);

        }
        Mail_Sender mail_Sender = new Mail_Sender();
        [Authorize(Roles = "C,B")]








        //Yorum
        [Authorize(Roles = "C,B")]
        public ActionResult Yorumlar(int statu = 1)
        {
            /*
             statu 1: onaylanmayı bekleyenler
                   2: tüm
                   3: onaylananlar

             
             */
            //Kullanıcı yetki sınama
            Kullanicilar kullanici = KullaniciOturumAcmis();
            if (kullanici != null && (kullanici.Yetki == "B" || kullanici.Yetki == "C"))
            {
                List<Yorumlar> model = null;
                if (statu == 1)
                {
                    model = DB.Yorumlar.Where(x => x.onayliMi == false).ToList();
                }
                else if (statu == 2)
                {
                    model = DB.Yorumlar.ToList();
                }
                else if (statu == 3)
                {
                    model = DB.Yorumlar.Where(x => x.onayliMi == true).ToList();
                }


                return View(model);
            }
            return HttpNotFound();
        }
        [HttpPost]
        [Authorize(Roles = "C,B")]
        public ActionResult getYorum(int id)
        {
            var model = DB.Yorumlar.FirstOrDefault(x => x.id == id);
            if (model != null)
                return PartialView(model);

            return HttpNotFound();
        }

        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void YorumOnaylaTek(int id)
        {
            var model = DB.Yorumlar.FirstOrDefault(x => x.id == id);
            if (model != null)
            {
                model.onayliMi = true;
                model.onaylanmaTarihi = DateTime.Now;
                if (KullaniciOturumAcmis() != null)
                    model.onaylayanId = KullaniciOturumAcmis().id;
                DB.SaveChanges();
            }
        }
        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void YorumOnaylaDizi(int[] id)
        {
            var model = DB.Yorumlar.Where(x => id.Contains(x.id)).ToList();
            Kullanicilar kullanici = KullaniciOturumAcmis();
            foreach (var item in model)
            {
                if (item != null)
                {
                    item.onayliMi = true;
                    item.onaylanmaTarihi = DateTime.Now;
                    if (kullanici != null)
                        item.onaylayanId = kullanici.id;

                }
            }
            DB.SaveChanges();
        }
        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void YorumSilTek(int id)
        {
            var model = DB.Yorumlar.FirstOrDefault(x => x.id == id);
            if (model != null)
            {
                DB.Yorumlar.Remove(model);
                DB.SaveChanges();
            }
        }
        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void YorumSilDizi(int[] id)
        {
            var model = DB.Yorumlar.Where(x => id.Contains(x.id)).ToList();
            foreach (var item in model)
            {
                if (item != null)
                {
                    DB.Yorumlar.Remove(item);

                }
            }
            DB.SaveChanges();
        }


        //İstatistik
        [Authorize(Roles = "C,B")]
        public ActionResult Istatistik()
        {
            /*
             filmSayisi
                resimsizFilmYuzdesi
             yorumSayisi
                onaysizResimYuzdesi
             */
            ViewData["filmSayisi"] = DB.Film.Count();
            if ((int)ViewData["filmSayisi"] == 0)
                ViewData["resimsizFilmYuzdesi"] = 0;
            else
                ViewData["resimsizFilmYuzdesi"] = 100 * DB.Film.Where(x => string.IsNullOrEmpty(x.ResimYol) || x.ResimYol == "null" || x.ResimYol == "NULL").Count() / (int)ViewData["filmSayisi"];

            
            ViewData["yorumSayisi"] = DB.Yorumlar.Count();
            if ((int)ViewData["yorumSayisi"] == 0)
                ViewData["onaysizYorumYuzdesi"] = 0;
            else
                ViewData["onaysizYorumYuzdesi"] = 100 * DB.Yorumlar.Where(x => x.onayliMi == false).Count() / (int)ViewData["yorumSayisi"];

            return View();
        }


        //Anasayfa slider
        [Authorize(Roles = "C,B")]
        public ActionResult SlaytSecimi()
        {
            var model = DB.anasayfaSlayt.ToList();
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "C,B")]
        public ActionResult getSlayt(int id)
        {

            

            var model = DB.anasayfaSlayt.FirstOrDefault(x => x.id == id);
            if (model == null)
            {
                //Film dropdown
                List<SelectListItem> filmListesi = (from k in DB.Film
                                                    select new SelectListItem
                                                    {
                                                        Text = k.Isim,
                                                        Value = k.id.ToString()
                                                    }).OrderBy(x=>x.Text).ToList();
                ViewBag.Liste = filmListesi;
                return PartialView(new anasayfaSlayt() { id=0});

            }
            return PartialView(model);

           
        }
        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void SlaytSil(int id)
        {
            var model = DB.anasayfaSlayt.FirstOrDefault(x => x.id == id);
            DB.anasayfaSlayt.Remove(model);
            DB.SaveChanges();
            //Serverdan resim silme
            ImageDelete(Server.MapPath(@"~/images/film/slide/" + model.filmId + ".jpg"));
            ImageDelete(Server.MapPath(@"~/images/film/slide/" + model.filmId + "_2.jpg"));
        }
        [HttpPost]
        [Authorize(Roles = "C,B")]
        public ActionResult SlaytEkle(anasayfaSlayt slayt,string url1,string url2)
        {
            if (!string.IsNullOrEmpty(url1) && !string.IsNullOrEmpty(url2))
            {
                int width = Convert.ToInt32(ConfigurationManager.AppSettings["imageWidth"]);
                int height = Convert.ToInt32(ConfigurationManager.AppSettings["imageHeight"]);
                string resimYol1 = Server.MapPath(@"~/images/film/slide/" + slayt.filmId + ".jpg");
                SaveImage(url1, resimYol1, ImageFormat.Jpeg);
                Image img1Gecici = Image.FromFile(resimYol1);
                Image img1 = ImageResizeImage(img1Gecici, new Size(width,height ));
                img1Gecici.Dispose();
                img1.Save(resimYol1);
                img1.Dispose();

                string resimYol2 = Server.MapPath(@"~/images/film/slide/" + slayt.filmId + "_2.jpg");
                SaveImage(url2, resimYol2, ImageFormat.Jpeg);
                Image img2Gecici = Image.FromFile(resimYol2);
                Image img2 = ImageResizeImage(img2Gecici, new Size(width, height));
                img2Gecici.Dispose();
                img2.Save(resimYol2);
                img2.Dispose();
                if (slayt != null)
                {
                    slayt.resimYoluHD="slide/"+slayt.filmId + ".jpg";
                    slayt.resimYoluHD_2 = "slide/" + slayt.filmId + "_2.jpg";
                    DB.anasayfaSlayt.Add(slayt);
                    DB.SaveChanges();
                    TempData["Hata"] = "Slayt kaydı başarılı";
                    

                }
                else
                    TempData["Hata"] = "Model boş girildi";
            }
            else
            {
                TempData["Hata"] = "Geçersiz URL girişi";
            }
            return RedirectToAction("SlaytSecimi");

        }

        [HttpPost]
        [Authorize(Roles = "C,B")]
        public ActionResult SlaytKaydet(anasayfaSlayt slayt2, string url1, string url2)
        {
            int width = Convert.ToInt32(ConfigurationManager.AppSettings["imageWidth"]);
            int height = Convert.ToInt32(ConfigurationManager.AppSettings["imageHeight"]);
            var slayt = DB.anasayfaSlayt.FirstOrDefault(x => x.id == slayt2.id);
            if (!string.IsNullOrEmpty(url1))
            {
                string resimYol1 = Server.MapPath(@"~/images/film/slide/" + slayt.filmId + ".jpg");
                SaveImage(url1, resimYol1, ImageFormat.Jpeg);
                Image img1Gecici = Image.FromFile(resimYol1);
                Image img1 = ImageResizeImage(img1Gecici, new Size(width, height));
                img1Gecici.Dispose();
                img1.Save(resimYol1);
                img1.Dispose();
                slayt.resimYoluHD = "slide/" + slayt.filmId + ".jpg";
            }
            if (!string.IsNullOrEmpty(url2))
            {
                string resimYol2 = Server.MapPath(@"~/images/film/slide/" + slayt.filmId + "_2.jpg");
                SaveImage(url2, resimYol2, ImageFormat.Jpeg);
                Image img2Gecici = Image.FromFile(resimYol2);
                Image img2 = ImageResizeImage(img2Gecici, new Size(width, height));
                img2Gecici.Dispose();
                img2.Save(resimYol2);
                img2.Dispose();
                slayt.resimYoluHD_2 = "slide/" + slayt.filmId + "_2.jpg";
            }
            slayt.onerimSekli = slayt2.onerimSekli;
            DB.SaveChanges();
            TempData["Hata"] = "Slayt kaydı başarılı";
            return RedirectToAction("SlaytSecimi");


        }

       


        //image
        public static Image ImageResizeImage(Image imgToResize, Size destinationSize)
        {
            if (imgToResize.Width == destinationSize.Width && imgToResize.Height == destinationSize.Height)
                return imgToResize;
            

            var originalWidth = imgToResize.Width;
            var originalHeight = imgToResize.Height;

            //how many units are there to make the original length
            var hRatio = (float)originalHeight / destinationSize.Height;
            var wRatio = (float)originalWidth / destinationSize.Width;

            //get the shorter side
            var ratio = Math.Min(hRatio, wRatio);

            var hScale = Convert.ToInt32(destinationSize.Height * ratio);
            var wScale = Convert.ToInt32(destinationSize.Width * ratio);

            //start cropping from the center
            var startX = (originalWidth - wScale) / 2;
            var startY = (originalHeight - hScale) / 2;

            //crop the image from the specified location and size
            var sourceRectangle = new Rectangle(startX, startY, wScale, hScale);

            //the future size of the image
            var bitmap = new Bitmap(destinationSize.Width, destinationSize.Height);

            //fill-in the whole bitmap
            var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            //generate the new image
            using (var g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
            }

            return bitmap;


        }
        public void SaveImage(string imageUrl, string filename, ImageFormat format)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imageUrl);
            Bitmap bitmap; bitmap = new Bitmap(stream);

            if (bitmap != null)
            {
                bitmap.Save(filename, format);
            }

            stream.Flush();
            stream.Close();
            client.Dispose();
        }
        public void ImageDelete(string path)
        {
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }


        //Erişim Kapat
        [Authorize(Roles = "C,B")]
        public ActionResult Erisim()
        {
            ViewData["ErisimAcik"] = (from k in DB.Film where k.ErisimEngelli == false
                                      select new SelectListItem
                                      {
                                          Text = k.Isim,
                                          Value = k.id.ToString()
                                      }).OrderBy(x => x.Text).ToList();

            ViewData["ErisimKapali"] = (from k in DB.Film where k.ErisimEngelli==true
                                        select new SelectListItem
                                        {
                                            Text = k.Isim,
                                            Value = k.id.ToString()
                                        }).OrderBy(x=>x.Text).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void ErisimKapat(int id) {
            var model = DB.Film.FirstOrDefault(x => x.id == id);
            model.ErisimEngelli = true;
            DB.SaveChanges();
        }
        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void ErisimAc(int id)
        {
            var model = DB.Film.FirstOrDefault(x => x.id == id);
            model.ErisimEngelli = false;
            DB.SaveChanges();
        }

        //Tür
        [Authorize(Roles = "C,B")]
        public ActionResult Tur()
        {
            var model = DB.FilmTur.ToList();
            return View(model);
        }
        [Authorize(Roles = "C,B")]
        [HttpGet]
        public void TurSil(int id)
        {
            var model = DB.FilmTur.FirstOrDefault(x => x.id == id);
            if(model != null)
            {
                foreach (var item in model.Film)
                {
                    item.FilmTur.Remove(model);
                }
                DB.FilmTur.Remove(model);
                DB.SaveChanges();
            }
        }
        [Authorize(Roles = "C,B")]
        [HttpPost]
        public ActionResult TurEkle(FilmTur tur)
        {
            if (tur != null)
            {
                DB.FilmTur.Add(tur);
                DB.SaveChanges();
                TempData["Hata"] = "Tür Başarıyla Eklendi.";
                return RedirectToAction("Tur");
            }
            TempData["Hata"] = "Null giriş yapıldı. Tür eklenemedi.";
            return RedirectToAction("Tur");
        }
        [Authorize(Roles = "C,B")]
        [HttpPost]
        public ActionResult TurDuzenle(FilmTur tur)
        {
            if (tur != null)
            {
                DB.FilmTur.FirstOrDefault(x => x.id == tur.id).TurAdi = tur.TurAdi;
                DB.SaveChanges();
                TempData["Hata"] = "Tür isim değişikliği başarıyla gerçekleşti.";
                return RedirectToAction("Tur");
            }
            TempData["Hata"] = "Null giriş yapıldı. Tür eklenemedi.";
            return RedirectToAction("Tur");
        }

        //İletişim Formu
        [Authorize(Roles = "C,B")]
        public ActionResult IletisimFormu(int statu = 1)
        {
            /*
             statu 1: okunmayı bekleyenler
                   2: tüm
                   3: okunanlar
             */
            //Kullanıcı yetki sınama
            Kullanicilar kullanici = KullaniciOturumAcmis();
            if (kullanici != null && (kullanici.Yetki == "B" || kullanici.Yetki == "C"))
            {
                List<IletisimFormu> model = null;
                if (statu == 1)
                {
                    model = DB.IletisimFormu.Where(x => x.OkunduMu == false).ToList();
                }
                else if (statu == 2)
                {
                    model = DB.IletisimFormu.ToList();
                }
                else if (statu == 3)
                {
                    model = DB.IletisimFormu.Where(x => x.OkunduMu == true).ToList();
                }


                return View(model);
            }
            return HttpNotFound();
        }
        [HttpPost]
        [Authorize(Roles = "C,B")]
        public ActionResult getIletisimFormu(int id)
        {
            var model = DB.IletisimFormu.FirstOrDefault(x => x.id == id);
            if (model != null)
                return PartialView(model);

            return HttpNotFound();
        }
        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void IletisimFormuOnayla(int id)
        {
            var model = DB.IletisimFormu.FirstOrDefault(x => x.id == id);
            if (model != null)
            {
                model.OkunduMu = true;
                DB.SaveChanges();
            }
        }
        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void IletisimFormuSil(int id)
        {
            var model = DB.IletisimFormu.FirstOrDefault(x => x.id == id);
            if (model != null)
            {
                DB.IletisimFormu.Remove(model);
                DB.SaveChanges();
            }
        }
        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void IletisimFormuOnaylama(int id)
        {
            var model = DB.IletisimFormu.FirstOrDefault(x => x.id == id);
            if (model != null)
            {
                model.OkunduMu = false;
                DB.SaveChanges();
            }
        }

        //Bildirilen Hatalar

        public class BildirilenHatalarModel
        {
            public int filmId { get; set; }
            public int count { get; set; }
        }
        [Authorize(Roles = "C,B")]
        public ActionResult BildirilenHatalar(int statu=1)
        {
            /*
            statu 1: incelenmeyenler
                  2: incelenenler
            */
            List<BildirilenHatalarModel> model = null;
            if (statu==1)
            {
                 model = DB.BildirilenHatalar.Where(x=>x.incelendiMi==false)
               .GroupBy(x => x.filmId)
               .Select(x => new BildirilenHatalarModel { count = x.Count(), filmId = x.Key }).ToList();
            }
            else
            {
                model = DB.BildirilenHatalar.Where(x => x.incelendiMi == true)
             .GroupBy(x => x.filmId)
             .Select(x => new BildirilenHatalarModel { count = x.Count(), filmId = x.Key }).ToList();
            }
           

            return View(model);
        }
        [Authorize(Roles = "C,B")]
        [HttpGet]
        public void BildirileriIncele(int filmId)
        {
            var model = DB.BildirilenHatalar.Where(x => x.incelendiMi == false && x.filmId == filmId).ToList();
            foreach (var item in model)
            {
                item.incelendiMi = true;
            }
            DB.SaveChanges();
        }
        [Authorize(Roles = "C,B")]
        [HttpPost]
        public ActionResult getBildirilerinMesajları(int filmId)
        {
            var model = DB.BildirilenHatalar.Where(x => x.filmId == filmId).ToList();
            return PartialView(model);
        }

        //Film Yükle

        [Authorize(Roles = "C,B")]
        public ActionResult FilmYukle()
        {
            List<SelectListItem> FilmTurListesi = (from k in DB.FilmTur
                                                    select new SelectListItem
                                                    {
                                                        Text = k.TurAdi,
                                                        Value = k.id.ToString()
                                                    }).ToList();
            ViewData["filmTurListesi"] = FilmTurListesi;

            return View();
        }
        [Authorize(Roles = "C,B")]
        [HttpPost]
        public ActionResult FilmYukle(Film film,string urlResim,string Turs)
        {
            if (film != null)
            {
                if(string.IsNullOrEmpty(Turs))
                    TempData["Hata"] = "Tür seçimi yapılmamış.";
                else
                {
                    if(string.IsNullOrEmpty(urlResim))
                        TempData["Hata"] = "Resim Url'si girilmemiş.";
                    else
                    {
                        //film girişi
                        film.eklenmeTarihi = DateTime.Now;
                        DB.Film.Add(film);
                        DB.SaveChanges();
                        //Tür girişi
                        foreach (var item in Turs.Split(','))
                        {
                            film.FilmTur.Add(DB.FilmTur.FirstOrDefault(x => x.TurAdi == item));
                        }
                        DB.SaveChanges();
                        //resim Girişi
                        string path = Server.MapPath(@"~/images/film/" + film.id + ".jpg");
                        SaveImage(urlResim, path, ImageFormat.Jpeg);
                        Image img1 = Image.FromFile(path);
                        Image imgGecici = ImageResizeImage(img1,new Size(Convert.ToInt32(ConfigurationManager.AppSettings["imageWidth"]), Convert.ToInt32(ConfigurationManager.AppSettings["imageHeight"])));
                        img1.Dispose();
                        imgGecici.Save(path);
                        imgGecici.Dispose();
                        film.ResimYol=film.id + ".jpg";
                        DB.SaveChanges();
                        TempData["Hata"] = "Film Başarıyla Eklendi";
                    }
                }
            }
            else
                TempData["Hata"] = "Null giriş yapıldı.";
            return RedirectToAction("FilmYukle");
        }


        //Filme Link Ekle

        [Authorize(Roles = "C,B")]
        public ActionResult FilmeLinkEkle()
        {
            List<SelectListItem> FilmListesi = (from k in DB.Film
                                                    select new SelectListItem
                                                    {
                                                        Text = k.Isim,
                                                        Value = k.id.ToString()
                                                    }).ToList();

            ViewData["filmListesi"] = FilmListesi;
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "C,B")]
        public ActionResult getFilmLink(int id)
        {
            var model = DB.Film.FirstOrDefault(x => x.id == id).IframeTablosu.ToList();
            return PartialView(model);
        }
        [HttpGet]
        [Authorize(Roles = "C,B")]
        public void FilmLinkSil(int id)
        {
            var model = DB.IframeTablosu.FirstOrDefault(x => x.id == id);
            if (model != null)
            {
                DB.IframeTablosu.Remove(model);
                DB.SaveChanges();
            }
        }
        [HttpPost]
        [Authorize(Roles = "C,B")]
        public void FilmeYeniLinkEkle(string kaynakIsim,string tekLink,string dublaj,string altyazi,int filmId)
        {
            var iframe = new IframeTablosu();
            iframe.kaynakIsmi = kaynakIsim;
            iframe.filmId = filmId;
            if (string.IsNullOrEmpty(tekLink))
            {
                iframe.VarsaTrDublaj = dublaj;
                iframe.VarsaTrAltyazi = altyazi;
            }
            else
            {
                iframe.kaynakLinki = tekLink;
            }
            DB.IframeTablosu.Add(iframe);
            DB.SaveChanges();
        }

        //Null resimleri doldur
        [Authorize(Roles ="C,B")]
        public ActionResult NullResimleriDoldur()
        {
            ViewData["countNull"] = DB.Film.Where(x => string.IsNullOrEmpty(x.ResimYol)).Count();
            return View();
        }
        [Authorize(Roles = "C,B")]
        [HttpPost]
        public ActionResult NullResimiDoldur(int filmIdInput,string urlResim)
        {
            var film = DB.Film.FirstOrDefault(x => x.id == filmIdInput);
            if (film != null && !string.IsNullOrEmpty(urlResim))
            {
                //Resim eklenebilir
                string path = Server.MapPath(@"~/images/film/" + film.id + ".jpg");
                SaveImage(urlResim, path, ImageFormat.Jpeg);
                Image img1 = Image.FromFile(path);
                Image imgGecici = ImageResizeImage(img1, new Size(Convert.ToInt32(ConfigurationManager.AppSettings["imageWidth"]), Convert.ToInt32(ConfigurationManager.AppSettings["imageHeight"])));
                img1.Dispose();
                imgGecici.Save(path);
                imgGecici.Dispose();
                film.ResimYol = film.id + ".jpg";
                DB.SaveChanges();
            }
            return RedirectToAction("NullResimleriDoldur");
        }

        [Authorize(Roles = "C,B")]
        [HttpPost]
        public ActionResult getNullResimFilm(int offset=0)
        {
            
                var model = DB.Film.Where(x => string.IsNullOrEmpty(x.ResimYol)).OrderBy(x=>x.id).Skip(offset).FirstOrDefault();
          
                return PartialView(model);
            
            
        }

        //Kullanıcı Engelle / Ban
        [Authorize(Roles = "C,B")]
        public ActionResult Ban()
        {
            return View();
        }
       
        [Authorize(Roles = "C,B")]
        [HttpGet]
        public ActionResult getKullanicilar(string ara)
        {
            var model = DB.Kullanicilar.Where(x => x.kAdi.Contains(ara) && x.Yetki == "A").ToList();
            return PartialView(model);
        }
        [Authorize(Roles = "C,B")]
        [HttpGet]
        public void Engelle(int id)
        {
            var model = DB.Kullanicilar.FirstOrDefault(x => x.id == id);
            if (model.Yetki == "A")
            {
                model.BAN = true;
            }
            DB.SaveChanges();
        }
        [Authorize(Roles = "C,B")]
        [HttpGet]
        public void EngelAc(int id)
        {
            var model = DB.Kullanicilar.FirstOrDefault(x => x.id == id);
            if (model.Yetki == "A")
            {
                model.BAN = false;
            }
            DB.SaveChanges();
        }

        //-----------------------------------------------> C seviye yetki

        //Yetki Ver
        [Authorize(Roles = "C")]
        public ActionResult YetkiVer()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "C")]
        public ActionResult getKullanicilarForYetki(string ara)
        {
            var model = DB.Kullanicilar.Where(x => x.kAdi.Contains(ara)).ToList();
            return PartialView(model);
        }
        [Authorize(Roles = "C")]
        public ActionResult yetkiDegistir(int id,string yetki)
        {
            var kllnci = DB.Kullanicilar.FirstOrDefault(x => x.id == id);
            if (kllnci != null)
            {
                if (yetki == "A" || yetki == "B" || yetki == "C")
                {
                    kllnci.Yetki = yetki;
                    DB.SaveChanges();
                    TempData["Hata"] = "Yetki değişikliği başarıyla gerçekleşmiştir";
                }
                else
                    TempData["Hata"] = "Geçersiz yetki girişi yapıldı";
            }
            else
                TempData["Hata"] = "Kullanıcı Bulunamadı";

            return RedirectToAction("YetkiVer");
        }

        //Veritabanı Temizliği
        [Authorize(Roles = "C")]
        public ActionResult VeritabaniTemizligi()
        {
            var model = DB.GonderilenMailler.ToList();
            return View(model);
        }
        [Authorize(Roles = "C")]
        [HttpPost]
        public ActionResult VeritabaniTemizligi(int statu)
        {
            //statu :1 -> işlemi gerçekleşmiş mailler
            //statu :2 -> zamanı geçmiş mailler
            if (statu == 1)
            {
                var model = DB.GonderilenMailler.Where(x => x.IslemGerceklestiMi).ToList();
                foreach (var item in model)
                    DB.GonderilenMailler.Remove(item);
                DB.SaveChanges();
                TempData["Hata"] = "İşlemi gerçekleşmiş mailler temizlendi";
               
            }else if (statu == 2)
            {
                var model = DB.GonderilenMailler.ToList();
                foreach (var item in model)
                {
                    if (DateTime.Compare(DateTime.Now, item.GonderimZamani.AddMinutes(item.GonderilenMailTipleri.ZamanAsimiSuresiDKCinsi)) > 0)
                        DB.GonderilenMailler.Remove(item);
                }
                DB.SaveChanges();
                TempData["Hata"] = "Son kullanım zamanı geçmiş mailler temizlendi";
            }
            else
            {
                TempData["Hata"] = "Geçersiz giriş.";
            }

            return RedirectToAction("VeritabaniTemizligi");
        }
       
    }
}