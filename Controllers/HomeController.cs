using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FilmFree.Models;
using System.Text.RegularExpressions;
using FilmFree.Controllers;
using System.Web.Security;
using PagedList;
using System.Net.Http;

namespace FilmFree.Controllers
{
    public class CaptchaResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }

    /*
     Datetime.Compare(tarih1,tarih2)
    <0 −  tarih1 tarih2'den önceyse
    =0 −  tarih1 tarih2 ile aynıysa
    >0 −  tarih1 tarih2'den sonraysa
     
     */

    public class HomeController : Controller
    {

        Mail_Sender mail_Sender = new Mail_Sender();
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

        CaptchaResult CapthcaRslt()
        {

            var captcha = Request.Form["g-recaptcha-response"];
            const string secret = "6LcQ7RobAAAAAPklaszjhlkp7sI6C5QwUX-Ih4I6";
            var restUrl = string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, captcha);
            WebRequest req = WebRequest.Create(restUrl);
            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
            JsonSerializer serializer = new JsonSerializer();
            CaptchaResult result = null;
            using (var reader = new StreamReader(resp.GetResponseStream()))
            {
                string resultObject = reader.ReadToEnd();
                result = JsonConvert.DeserializeObject<CaptchaResult>(resultObject);
            }


            return result;
        }

        public ActionResult Index()
        {
            ViewData["Slayt"] = DB.anasayfaSlayt.ToList();
            ViewData["EnCokIzlenen"] = DB.Film.OrderByDescending(x => x.IzlenmeSayisi).Take(8).ToList();
            ViewData["EnCokBegenilen"] = DB.Film.OrderByDescending(x => x.BegenilmeSayisi).Take(8).ToList();
            ViewData["Rastgele"] = DB.Film.OrderBy(t => Guid.NewGuid()).Take(8).ToList();


            //Beğenilen Filmleri Gönderme
            Kullanicilar kllnci = KullaniciOturumAcmis();
            if (kllnci != null)
                ViewData["begenilenFilmIdler"] = (List<int>)kllnci.Film.Select(x => x.id).ToList();

            return View();
        }
        public ActionResult Iletisim()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Iletisim(IletisimFormu form)
        {




            if (!CapthcaRslt().Success)
                TempData["Hata"] = "CAPTCHA Doğrulamasını Sağlayınız.";

            else
            {
                if (form == null)
                    TempData["Hata"] = "Kayıda NULL değer girildi!";

                else
                {
                    if (form.Ad == null || form.Ad.Length > 50)
                        TempData["Hata"] = "Ad değeri NULL veya 50 karakterden uzun.";

                    else
                    {
                        if (form.Mail == null || form.Mail.Length > 50)
                            TempData["Hata"] = "Mail değeri NULL veya 50 karakterden uzun.";

                        else
                        {
                            if (form.Konu == null || form.Konu.Length > 50)
                                TempData["Hata"] = "Konu değeri NULL veya 50 karakterden uzun.";

                            else
                            {
                                if (form.Mesaj == null || form.Mesaj.Length > 500)
                                    TempData["Hata"] = "Ad değeri NULL veya 500 karakterden uzun.";

                                else
                                {
                                    form.Tarih = DateTime.Now;
                                    DB.IletisimFormu.Add(form);
                                    DB.SaveChanges();
                                    TempData["Hata"] = "Kayıt Başarılı.";
                                }
                            }
                        }
                    }
                }

            }

            return View();
        }

        public ActionResult Hukuksal()
        {
            return View();
        }
        public ActionResult Filmler()
        {
            ViewData["idOfFilmTur"] = DB.FilmTur.ToList();
            foreach (var item in (List<FilmTur>)ViewData["idOfFilmTur"])
            {
                int filmCount = DB.Film.Where(x => x.FilmTur.FirstOrDefault().id == item.id).Count();
                if (filmCount >= 8)
                {
                    filmCount = 8;
                    ViewData["FilmTur" + item.id] = (List<Film>)DB.Film.Where(x => x.FilmTur.FirstOrDefault().id == item.id).OrderBy(t => Guid.NewGuid()).Take(filmCount).ToList();

                }
                else
                {
                    int eklenecekFilmSayisi = DB.Film.Where(x => x.FilmTur.Select(y => y.id).ToList().Contains(item.id)).Count();
                    if (eklenecekFilmSayisi > 8)
                        eklenecekFilmSayisi = 8;
                    ViewData["FilmTur" + item.id] = (List<Film>)DB.Film.Where(x => x.FilmTur.Select(y => y.id).ToList().Contains(item.id)).OrderBy(t => Guid.NewGuid()).Take(eklenecekFilmSayisi).ToList();
                }
            }

            //Beğenilen Filmleri Gönderme
            Kullanicilar kllnci = KullaniciOturumAcmis();
            if (kllnci != null)
                ViewData["begenilenFilmIdler"] = (List<int>)kllnci.Film.Select(x => x.id).ToList();




            return View();
        }
      

        public ActionResult Kaydol()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Kaydol(Kullanicilar kllnci, string SifreTekrari)
        {
            if (kllnci != null)
            {
                if (CapthcaRslt().Success)
                {
                    if (kllnci.Sifre != null && SifreTekrari == kllnci.Sifre)
                    {
                        if (kllnci.Sifre.Length >= 8 && kllnci.Sifre.Length <= 20)
                        {
                            //Şifre kontrolü tamamlandı
                            if (kllnci.Mail != null && kllnci.Mail.Length < 50 && DB.Kullanicilar.FirstOrDefault(x => x.Mail == kllnci.Mail) == null)
                            {//mail null değilse ve 50 karakterden kısaysa ve aynı mail adresi daha önceden kullanılmamışsa

                                if (kllnci.kAdi.Length >= 8 && kllnci.kAdi.Length <= 20)
                                {
                                    Regex r = new Regex(@"[A-Za-z][A-Za-z0-9._]");
                                    if (r.IsMatch(kllnci.kAdi))
                                    {
                                        //Kullanıcı kaydı yapılabilir.
                                        kllnci.KayitTarihi = DateTime.Now;
                                        kllnci.Yetki = "A";
                                        kllnci.Dogrulama = Guid.NewGuid().ToString();
                                        DB.Kullanicilar.Add(kllnci);
                                        DB.SaveChanges();

                                        GonderilenMailler mail = new GonderilenMailler
                                        {
                                            GonderimZamani = DateTime.Now,
                                            GonderimTipiId = DB.GonderilenMailTipleri.FirstOrDefault(x => x.TipAdi == "MailOnay").id,
                                            KullaniciId = DB.Kullanicilar.FirstOrDefault(x => x.kAdi == kllnci.kAdi).id,
                                            token = Guid.NewGuid().ToString()
                                        };
                                        DB.GonderilenMailler.Add(mail);
                                        DB.SaveChanges();


                                        /////BURADA KALDIN
                                        if (!mail_Sender.SendMail(kllnci.Mail, "Mail Aktivasyon", "Mailinizi aktif etmek için linke tıklayınız:" + "<a href='" + HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + "/Home/Aktivasyon?username=" + mail.Kullanicilar.kAdi + "&token=" + mail.token + "'>" + HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + "/Home/Aktivasyon?username=" + mail.Kullanicilar.kAdi + "&token=" + mail.token + "</a>" + "</br><h4>FilmFree Ekibi</h4>"))
                                            TempData["Hata"] = "Onay Maili Gönderilirken Hata.";
                                        else
                                            TempData["Hata"] = "Kullanıcı kaydı başarıyla gerçekleşti. Mail adresinizi onaylamak için Gelen Kutunuzu ve Spam Kutunuzu kontrol edin.";


                                    }
                                    else
                                        TempData["Hata"] = "Kullanıcı adında harf, sayı ve . _ karakterlerinden başka özel karakter kullanılamaz. Kullanıcı adı sayı ile başlayamaz.";

                                }
                                else
                                    TempData["Hata"] = "Kullanıcı adı 8-20 karakter arasında olmalıdır.";
                            }
                            else
                                TempData["Hata"] = "Mail adresi daha önce kullanılmış veya geçersiz mail adresi girildi.";


                        }
                        else
                            TempData["Hata"] = "Şifreniz 8 karakterden kısa veya 20 karakterden uzun.";
                    }
                    else
                        TempData["Hata"] = "Şifreler Uyuşmuyor.";

                }
                else
                    TempData["Hata"] = "CAPTHCA Doğrulamasını Sağlayınız.";
            }
            else
                TempData["Hata"] = "NULL giriş yapıldı.";
            return View();
        }
        [HttpGet]
        public ActionResult Aktivasyon(string username, string token)
        {
            if (username == null || token == null)
                TempData["Hata"] = "Geçersiz URL";
            else
            {
                GonderilenMailler mail = DB.GonderilenMailler.FirstOrDefault(x => x.Kullanicilar.kAdi == username && x.token == token);


                if (mail == null)
                    TempData["Hata"] = "Geçersiz Kullanıcı adı.";
                else
                {
                    if (mail.IslemGerceklestiMi)
                        TempData["Hata"] = "Aktivasyon linki zaten kullanılmış.";
                    else
                    {
                        DateTime sonGecerlilikZamanı = mail.GonderimZamani;
                        sonGecerlilikZamanı = sonGecerlilikZamanı.AddMinutes(mail.GonderilenMailTipleri.ZamanAsimiSuresiDKCinsi);
                        if (DateTime.Compare(DateTime.Now, sonGecerlilikZamanı) > 0)
                            TempData["Hata"] = "Aktivasyon linki kullanım süresi aşılmış. Kullanım süresi:" + mail.GonderilenMailTipleri.ZamanAsimiSuresiDKCinsi + " DK'dır. Tekrardan mail gönderilmesi için lütfen GİRİŞ sayfasını kontrol ediniz.";

                        else
                        {

                            //işlem gerçekleştirilebilinir
                            mail.IslemGerceklestiMi = true;

                            //GonderimTipiId=1 => Mail Onayı
                            //              =2 => Şifre Sıfırlama
                            if (mail.GonderimTipiId == 1)
                                mail.Kullanicilar.MailOnayliMi = true;
                            else
                            {
                                //şifre sıfırla
                                Random rastgele = new Random();
                                string harfler = "ABCDEFGHIJKLMNOPRSTUVYZabcdefghijklmnoprstuvyz";
                                string newSifre = "";
                                for (int i = 0; i < 8; i++)
                                    newSifre += harfler[rastgele.Next(harfler.Length)];
                                mail.Kullanicilar.Sifre = newSifre;
                                if (!mail_Sender.SendMail(mail.Kullanicilar.Mail, "Yeni Şifre", "<h3>Şifreniz Sıfırlanmıştır.</h3></br>Giriş Yapmak İçin Kullanacağınız Yeni Şifreniz:" + newSifre + "</br>Şifrenizi Hesabım Sekmesinden Değiştirebilirsiniz.</br><h4>FilmFree Ekibi</h4>"))
                                {
                                    TempData["Hata"] = "Yeni Şifre Maile Gönderilirken Hata. SMTP sunucunda arıza olabilir.";
                                    return RedirectToAction("Index");
                                }

                            }
                            DB.SaveChanges();
                            if (mail.GonderimTipiId == 1)
                                TempData["Hata"] = "Mail Aktivasyonunuz Başarıyla Gerçekleşmiştir.";
                            else
                                TempData["Hata"] = "Şifre Sıfırlama İşleminiz Başarıyla Gerçekleşmiştir. Yeni Şifrenizi Öğrenmek İçin E-Posta Adresinizin Gelen Kutusunu ve Spam Kutusunu Kontrol Ediniz";


                        }
                    }
                }
            }
            return RedirectToAction("Index");
        }
        public ActionResult GirisYap()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GirisYap(Kullanicilar kllnci, bool BeniHatirla)
        {
            if (!CapthcaRslt().Success)
                TempData["Hata"] = "CAPTHCA Doğrulamasını Sağlayınız.";
            else
            {
                if (kllnci == null)
                    TempData["Hata"] = "NULL giriş yapıldı.";
                else
                {
                    if (DB.Kullanicilar.FirstOrDefault(x => x.kAdi == kllnci.kAdi) == null)
                        TempData["Hata"] = "Böyle bir kullanıcı bulunamadı.";
                    else
                    {
                        Kullanicilar kllnciAsil = DB.Kullanicilar.FirstOrDefault(x => x.kAdi == kllnci.kAdi);
                        if (kllnciAsil.Sifre != kllnci.Sifre)
                            TempData["Hata"] = "Yanlış şifre girildi.";
                        else
                        {
                            if (!kllnciAsil.MailOnayliMi)
                                TempData["Hata"] = "Mail onaylanmadan giriş yapılamaz!";
                            else
                            {
                                if (kllnciAsil.BAN)
                                    TempData["Hata"] = "Kullanıcı Hesabınız Engellenmiştir.";
                                else
                                {
                                    FormsAuthentication.SetAuthCookie(kllnciAsil.kAdi + "," + kllnciAsil.Yetki + "," + kllnciAsil.Dogrulama, BeniHatirla);
                                    return RedirectToAction("Index");
                                }
                            }
                        }
                    }
                }
            }
            return View();
        }
        [Authorize]
        public ActionResult CikisYap()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
        public ActionResult TekrarMailAktivasyonuGonder()
        {
            return View();
        }
        [HttpPost]
        public ActionResult TekrarMailAktivasyonuGonder(string email, string tip)
        {
            //tip gelebilecek stringler: mail, sifre
            if (!CapthcaRslt().Success)
                TempData["Hata"] = "CAPTHCA Doğrulamasını Sağlayınız.";
            else
            {
                if (email == null || tip == null)
                    TempData["Hata"] = "Null Giriş Yapıldı.";
                else
                {
                    Kullanicilar kllnci = DB.Kullanicilar.FirstOrDefault(x => x.Mail == email);
                    if (kllnci == null)
                        TempData["Hata"] = "Girilen Maile Ait Bir Hesap Bulunamadı.";
                    else
                    {


                        if (tip == "mail" && kllnci.MailOnayliMi)
                        {
                            TempData["Hata"] = "Bu Mail Adresine Ait Hesap Zaten Onaylı.";
                            return View();
                        }
                        ///////////
                        List<GonderilenMailler> mail = new List<GonderilenMailler>();
                        //GonderimTipiId=1 => Mail Onayı
                        //              =2 => Şifre Sıfırlama

                        if (tip == "sifre")
                            mail = DB.GonderilenMailler.Where(x => x.KullaniciId == kllnci.id && x.GonderimTipiId == 2 && x.IslemGerceklestiMi == false).ToList();
                        else if (tip == "mail")
                            mail = DB.GonderilenMailler.Where(x => x.KullaniciId == kllnci.id && x.GonderimTipiId == 1 && x.IslemGerceklestiMi == false).ToList();
                        else
                        {
                            TempData["Hata"] = "Geçersiz Tip Girişi Yapıldı.";
                            return View();
                        }


                        //Daha önce mail gönderilmiş mi? Gönderilmiş ise zaman aşımı gerçekleşmiş mi?
                        bool zamanAsilmamisIse = false;
                        foreach (var item in mail)
                        {
                            if (DateTime.Compare(item.GonderimZamani.AddMinutes(item.GonderilenMailTipleri.ZamanAsimiSuresiDKCinsi), DateTime.Now) > 0)
                            //tarih1 tarih2'den sonraysa. Yani daha zaman aşımı gerçekleşmemiş ise
                            {
                                zamanAsilmamisIse = true;
                                break;
                            }
                        }
                        if (zamanAsilmamisIse)
                        {
                            ///////////
                            string mailOnaySuresiDkCinsi = "";
                            if (tip == "sifre")
                                mailOnaySuresiDkCinsi = DB.GonderilenMailTipleri.FirstOrDefault(x => x.id == 2).ZamanAsimiSuresiDKCinsi.ToString();
                            else
                                mailOnaySuresiDkCinsi = DB.GonderilenMailTipleri.FirstOrDefault(x => x.id == 1).ZamanAsimiSuresiDKCinsi.ToString();
                            TempData["Hata"] = "Zaten şifre sıfırlama bilgileri E-Postanıza gönderilmiş. Yeni E-posta almak için son gönderimin üzerinden " + mailOnaySuresiDkCinsi + " DK geçmesi gerekir. Lütfen Spam kutunuzu kontrol ediniz.";
                            return View();
                        }
                        else
                        {
                            //Mail Onayı Gönderilebilir.
                            GonderilenMailler onayMail = new GonderilenMailler
                            {
                                GonderimZamani = DateTime.Now,
                                ///////////
                                GonderimTipiId = 2
                            };
                            if (tip == "mail")
                                onayMail.GonderimTipiId = 1;
                            onayMail.KullaniciId = DB.Kullanicilar.FirstOrDefault(x => x.kAdi == kllnci.kAdi).id;
                            onayMail.token = Guid.NewGuid().ToString();
                            DB.GonderilenMailler.Add(onayMail);


                            //şimdi gerçek mail gönderilebilebilinir.
                            if (tip == "mail")
                            {
                                if (!mail_Sender.SendMail(kllnci.Mail, "Mail Aktivasyon", "Mailinizi aktif etmek için linke tıklayınız:" + "<a href='" + HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + "/Home/Aktivasyon?username=" + onayMail.Kullanicilar.kAdi + "&token=" + onayMail.token + "'>" + HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + "/Home/Aktivasyon?username=" + onayMail.Kullanicilar.kAdi + "&token=" + onayMail.token + "</a>" + "</br><h4>FilmFree Ekibi</h4>"))
                                {
                                    TempData["Hata"] = "Onay Maili Gönderilirken Hata. SMTP sunucunda arıza olabilir.";
                                    return View();
                                }
                            }
                            else
                            {
                                if (!mail_Sender.SendMail(kllnci.Mail, "Şifre Sıfırla", "Şifrenizi Sıfırlamak İçin Linke Tıklayınız:" + "<a href='" + HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + "/Home/Aktivasyon?username=" + onayMail.Kullanicilar.kAdi + "&token=" + onayMail.token + "'>" + HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + "/Home/Aktivasyon?username=" + onayMail.Kullanicilar.kAdi + "&token=" + onayMail.token + "</a>" + "</br>Eğer Şifre Sıfırlama İsteğini Siz Göndermediyseniz Bu Maili Dikkate Almayınız.</br><h4>FilmFree Ekibi</h4>"))
                                {
                                    TempData["Hata"] = "Şifre Sıfırlama Maili Gönderilirken Hata. SMTP sunucunda arıza olabilir.";
                                    return View();
                                }
                            }


                            DB.SaveChanges();
                            TempData["Hata"] = "Gerekli bilgiler E-Posta adresinize gönderilmiştir. Gelen kutunuzu ve Spam kutunuzu kontrol ediniz.";
                            return View();
                        }

                    }
                }
            }
            return View();
        }

        [Authorize]
        public ActionResult Hesabim()
        {
            try
            {
                if (KullaniciOturumAcmis() != null)
                    return View(KullaniciOturumAcmis());
                return HttpNotFound();
            }
            catch
            {
                return HttpNotFound();
            }


        }
        [Authorize]
        [HttpPost]
        public ActionResult Hesabim(string ad, string soyad, string sifre, string sifreTekrari, string eskiSifre)
        {
            Kullanicilar kllnci = KullaniciOturumAcmis();
            if (kllnci == null)
                return HttpNotFound();
            if (eskiSifre != kllnci.Sifre)
                TempData["Hata"] = "Mevcut şifreniz yanlış girildi.";
            else
            {
                if (ad != kllnci.Ad || soyad != kllnci.Soyad)
                {
                    if (ad.Length > 50 || soyad.Length > 50)
                        TempData["Hata"] = "Ad-Soyad 50 karakterden uzun!";
                    else
                    {
                        kllnci.Ad = ad;
                        kllnci.Soyad = soyad;
                        DB.SaveChanges();
                        TempData["Hata"] = "Ad-Soyad Değişikliği Başarıyla Gerçekleşmiştir.";
                    }

                }
                if (sifre != null && sifre != "")
                {
                    if (sifre != sifreTekrari)
                    {
                        if (TempData["Hata"] == null)
                            TempData["Hata"] = "";
                        TempData["Hata"] += "</br>Yeni şifreniz ve Yeni Şifre Tekrarı aynı değil!";
                    }

                    else
                    {
                        if (TempData["Hata"] == null)
                            TempData["Hata"] = "";

                        if (sifre.Length < 8 || sifre.Length > 20)
                            TempData["Hata"] += "Şifreniz 8 ila 20 karakter uzunluğunda olabilir!";
                        else
                        {
                            kllnci.Sifre = sifre;
                            DB.SaveChanges();

                            TempData["Hata"] += "Şifreniz Başarıyla Değiştirilmiştir!";
                        }

                    }
                }
            }

            if (KullaniciOturumAcmis() != null)
                return View(KullaniciOturumAcmis());
            else
                return HttpNotFound();
        }
        [HttpGet]
        public ActionResult Filtrele(string Encok, string Tur, string Ara, int sayfa = 1)
        {

            //Beğenilen Filmleri Gönderme
            Kullanicilar kllnci = KullaniciOturumAcmis();
            if (kllnci != null)
                ViewData["begenilenFilmIdler"] = (List<int>)kllnci.Film.Select(x => x.id).ToList();



            PagedList.IPagedList<Film> filmler = null;
            if (Encok == "Izlenenler")
            {
                filmler = DB.Film.OrderByDescending(x => x.IzlenmeSayisi).ToList().ToPagedList(sayfa, 16);
                ViewData["ttle"] = "En Çok İzlenen Filmler";
            }
            else if (Encok == "Begenilenler")
            {
                filmler = DB.Film.OrderByDescending(x => x.BegenilmeSayisi).ToList().ToPagedList(sayfa, 16);
                ViewData["ttle"] = "En Beğenilen Filmler";
            }
            else if (Encok == "Eklenen")
            {
                filmler = DB.Film.OrderByDescending(x => x.eklenmeTarihi).ToList().ToPagedList(sayfa, 16);
                ViewData["ttle"] = "En Beğenilen Filmler";
            }
            else if (Tur != null && Tur != "")
            {

                filmler = DB.FilmTur.FirstOrDefault(x => x.id.ToString() == Tur).Film.ToList().ToPagedList(sayfa, 16);
                ViewData["ttle"] = DB.FilmTur.FirstOrDefault(x => x.id.ToString() == Tur).TurAdi + " Türü Filmler";
            }
            else if (Ara != null && Ara != "")
            {
                var filmler_1 = DB.Film.Where(x => x.Isim.Contains(Ara) || x.Oyuncular.Contains(Ara) || x.Tags.Contains(Ara)).ToList();
                if (DB.FilmTur.FirstOrDefault(x => x.TurAdi.Contains(Ara)) != null)
                    filmler_1.AddRange(DB.FilmTur.FirstOrDefault(x => x.TurAdi.Contains(Ara)).Film.ToList());
                filmler = filmler_1.ToPagedList(sayfa, 16);

                ViewData["ttle"] = "Arama Sonuçları";
            }
            else
                ViewData["ttle"] = "Bulunamadı";
            return View(filmler);


        }

        [HttpPost]
        //[Authorize]
        public string FilmiBegen(int id)
        {
            Kullanicilar kllnci = KullaniciOturumAcmis();
            if (kllnci == null)
                return "false";
            else
            {
                if (kllnci.Film.Where(x => x.id == id).FirstOrDefault() == null)
                {
                    //Beğenilmemiş
                    //O halde ekleyelim
                    Film film = DB.Film.FirstOrDefault(x => x.id == id);
                    kllnci.Film.Add(film);
                    film.BegenilmeSayisi += 1;
                    DB.SaveChanges();

                    return film.BegenilmeSayisi.ToString() + " <span class='fa fa-heart'></span>";

                }
                else
                {
                    //Beğenilmiş
                    //O halde silelim
                    Film film = DB.Film.FirstOrDefault(x => x.id == id);
                    film.BegenilmeSayisi -= 1;
                    kllnci.Film.Remove(film);
                    DB.SaveChanges();
                    return film.BegenilmeSayisi.ToString() + " <span class='wp_ulike_btn wp_ulike_put_image'></span>";
                }
            }
        }

        public ActionResult Izle(int id)
        {
            Film film = DB.Film.FirstOrDefault(x => x.id == id);
            if (film == null || film.ErisimEngelli)
                return HttpNotFound();
            ViewData["ttle"] = film.Isim;

            //Yorum Gönderimi
            List<Yorumlar> yormlar = DB.Yorumlar.Where(x => x.filmId == id && x.onayliMi == true).OrderByDescending(x => x.Tarih).Take(3).ToList();
            ViewData["yorumlar"] = yormlar;

            //Kullanıcı gönderimi
            ViewData["kullanici"] = KullaniciOturumAcmis();

            //Benzer film Gönderimi
            int idOfTur = film.FilmTur.FirstOrDefault().id;
            ViewData["benzerFilm"] = film.FilmTur.FirstOrDefault().Film.Take(9).ToList();

            ((List<Film>)ViewData["benzerFilm"]).Remove(film);


            return View(film);
        }

        public ActionResult YorumEkle(string Ad, string Mail, string Yorum, int id, string returnUrl)
        {

            if (!CapthcaRslt().Success)
                TempData["Hata"] = "CAPTHCA Doğrulamasını Sağlayınız.";
            else
            {

                if (!string.IsNullOrEmpty(Ad) && Ad.Length < 50)
                {
                    if (!string.IsNullOrEmpty(Mail) && Mail.Length < 50)
                    {
                        if (!string.IsNullOrEmpty(Yorum) && Yorum.Length < 200)
                        {
                            //Kontroller tamam. Yorum eklenebilir
                            Yorumlar yorum = new Yorumlar();
                            yorum.Ad = Ad;
                            yorum.Mail = Mail;
                            yorum.Yorum = Yorum;
                            yorum.Tarih = DateTime.Now;
                            yorum.filmId = id;
                            //Filmin yorum sayısını arttırdık
                            DB.Film.FirstOrDefault(x => x.id == yorum.filmId).YorumSayisi++;
                            DB.Yorumlar.Add(yorum);
                            DB.SaveChanges();
                            TempData["Hata"] = "Yorumunuz kaydedilmiştir. Onaylandıktan sonra yayınlanacaktır.";

                        }
                        else
                            TempData["Hata"] = "Yorum maksimum 200 karakter olabilir.";
                    }
                    else
                        TempData["Hata"] = "Mail maksimum 50 karakter olabilir.";
                }
                else
                    TempData["Hata"] = "Ad maksimum 50 karakter olabilir.";

            }
            return Redirect(returnUrl);
        }

        [HttpPost]
        public string YorumGoruntule(int filmId, int sayfa = 1)
        {
            var yorumlar = DB.Yorumlar.Where(x => x.filmId == filmId && x.onayliMi == true).OrderByDescending(x => x.Tarih).ToList().ToPagedList(sayfa, 3);
            string rtrn = "";
            if (yorumlar != null && yorumlar.Count > 0)
            {

                foreach (var item in yorumlar)
                {
                    rtrn += "<li><span class='isim'>" + item.Ad + "</span>";
                    rtrn += "<time class='tarih'>" + item.Tarih.ToShortDateString() + "</time>";
                    rtrn += "<p>" + item.Yorum + "</p></li>";
                }

            }

            return rtrn;
        }
        [HttpPost]
        public ActionResult HataBildir(int id, string Aciklama, string returnUrl)
        {

            if (!CapthcaRslt().Success)
                TempData["Hata"] = "CAPTHCA Doğrulamasını Sağlayınız.";
            else
            {
                BildirilenHatalar hata = new BildirilenHatalar();
                if (!String.IsNullOrEmpty(Aciklama))
                {
                    if (Aciklama.Length < 200)
                    {
                        hata.Aciklama = Aciklama;
                    }
                    else
                        TempData["Hata"] = "Açıklama 200 karakterden uzun olamaz.";
                    hata.filmId = id;
                    hata.Tarih = DateTime.Now;
                    DB.BildirilenHatalar.Add(hata);
                    DB.SaveChanges();
                    TempData["Hata"] = "Geri bildiriminiz gönderilmiştir. Teşekkür ederiz.";
                }
                else
                    TempData["Hata"] = "Açıklama yapılmayan bildiriler dikkate alınmamaktır.";
            }

            return Redirect(returnUrl);
        }
     
        [HttpPost]//iframelerin linkini return ediyor. Çok kurcalama
        public string src(int iframeId, bool altyaziSecildi = false)
        {
            IframeTablosu iframe = DB.IframeTablosu.FirstOrDefault(x => x.id == iframeId);
            string uri = "";
            if (!string.IsNullOrEmpty(iframe.kaynakLinki) && iframe.kaynakLinki != "null" && iframe.kaynakLinki != "NULL")
            {
                //ViewData["altyaziDublajButonuVar"] = false;
                uri = iframe.kaynakLinki;
                return uri + "," + iframe.kaynakIsmi;

            }
            else
            {
                //ViewData["altyaziDublajButonuVar"] = true;
                
                if (altyaziSecildi)
                    uri = iframe.VarsaTrAltyazi;
                else
                    uri = iframe.VarsaTrDublaj;
                return uri + "," + iframe.kaynakIsmi+",var";

            }

            
            
        }








      [Authorize]
        public ActionResult Begendiklerim(int sayfa = 1)
        {
            Kullanicilar kullanici = KullaniciOturumAcmis();
            if (kullanici != null)
            {
                var model = kullanici.Film.ToList().ToPagedList(sayfa, 16);
                return View(model);

            }
            return HttpNotFound();
        }







    }



}
