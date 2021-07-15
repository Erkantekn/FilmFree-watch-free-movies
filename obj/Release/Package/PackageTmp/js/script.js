
/*====================================
[  Table of contents  ]
======================================
==> Page Loader
==> Search Button
==> Sticky Header
==> Back To Top
======================================
[ End table content ]
======================================
*/

//Ajax ile film beðenme iþlemi

$(function () {
    $(".gen-movie-add").on("click", "#filmBegen", function () {
        //Begenme butonuna basildi:

        //film id'si çekildi
        var id = $(this).data("id");
        var btn = $(this);
                $.ajax({
                    type: "POST",
                    url: "/Home/FilmiBegen?id=" + id,
                    success: function (data) {
                        if (data == "false") {
                            window.location.href = "/Home/GirisYap";
                        }
                        else {
                            //Giriþ yapýlmýþ ve iþlem gerçekleþti
                            
                            btn.children().remove();
                            btn.html(data);
                        }
                    }, error:function () {
                        window.location.href = "/Home/GirisYap";
                    }
                    
                        });
    
                
            });
        });

//Ajax ile yorum görüntüleme
$(function () {
    $(".col-xl-12.yorumlar").on("click", "#yorumGoruntule", function () {
        //Daha fazla göster butonuna basýldý
        
        //bilgiler çekildi
        var btn = $(this);
        var filmId = btn.data("filmid");
        var sayfa = btn.data("sayfa");
        sayfa += 1;
        //sayfa 1 arttýrýldý ve butona geri yazýldý
        btn.data("sayfa",sayfa);
        
        $.ajax({
            type: "POST",
            url: "/Home/YorumGoruntule?filmId=" + filmId + "&sayfa=" + sayfa,
            success: function (data) {
               
                //baþarýyla veri çekilirse
                //children().eq(1) => çocuklarýndan index numarasý 1 olan demektir
                if (data == "") {
                    btn.parent().parent().remove();
                }
                else {
                    btn.parent().parent().parent().children().eq(1).append(data);   
                }
                       
                
            }, error: function () {
                btn.parent().parent().parent().children().eq(1).html("Yorumlar yüklenirken hata oluþtu"); 
            }

        });


    });
});

//Ýzle Viewinda hata bildir modalý
$(function () {
    $(".col-lg-12").on("click", "#hataBildir", function () {
        var modal = document.getElementById("hataBildirModal");
        modal.style.display = "block";
    });
});



(function(jQuery) {
    "use strict";
    jQuery(window).on('load', function(e) {

        jQuery('p:empty').remove();

        /*------------------------
                Page Loader
        --------------------------*/
        jQuery("#gen-loading").fadeOut();
        jQuery("#gen-loading").delay(0).fadeOut("slow");

        /*------------------------
                Search Button
        --------------------------*/
        jQuery('#gen-seacrh-btn').on('click', function() {
            jQuery('.gen-search-form').slideToggle();
            jQuery('.gen-search-form').toggleClass('gen-form-show');
            if (jQuery('.gen-search-form').hasClass("gen-form-show")) {
                jQuery(this).html('<i class="fa fa-times"></i>');
            } else {
                jQuery(this).html('<i class="fa fa-search"></i>');
            }
        });

        jQuery('.gen-account-menu').hide();
         jQuery('#gen-user-btn').on('click', function(e) {
            
            jQuery('.gen-account-menu').slideToggle();

             e.stopPropagation();
        });

        jQuery('body').on('click' , function(){
            if(jQuery('.gen-account-menu').is(":visible"))
            {
                jQuery('.gen-account-menu').slideUp();
            }
        });
       
        /*------------------------
                Sticky Header
        --------------------------*/
        var view_width = jQuery(window).width();
        if (!jQuery('header').hasClass('gen-header-default') && view_width >= 1023)
        {
            var height = jQuery('header').height();
            jQuery('.gen-breadcrumb').css('padding-top', height * 1.3);
        }
        if (jQuery('header').hasClass('gen-header-default')) {

            jQuery(window).scroll(function () {
                var scrollTop = jQuery(window).scrollTop();
                if (scrollTop > 250) {
                    jQuery('.gen-bottom-header').addClass('gen-header-sticky animated fadeInDown animate__faster');
                }
                if (scrollTop < 150) {
                    jQuery('.gen-bottom-header').removeClass('gen-header-sticky animated fadeInDown animate__faster');
                }
            });
        }
        if (jQuery('header').hasClass('gen-has-sticky')) {
            jQuery(window).scroll(function() {
                var scrollTop = jQuery(window).scrollTop();
                
                if (scrollTop > 300 ) {
                    jQuery('header').addClass('gen-header-sticky animated fadeInDown animate__faster');
                }
                if (scrollTop<200) {
                    jQuery('header').removeClass('gen-header-sticky animated fadeInDown animate__faster');
                }
            });
        }
     
        /*------------------------
                Back To Top
        --------------------------*/
        jQuery('#back-to-top').fadeOut();
        jQuery(window).on("scroll", function() {
            if (jQuery(this).scrollTop() > 250) {
                jQuery('#back-to-top').fadeIn(1400);
            } if (jQuery(this).scrollTop()<150) {
                jQuery('#back-to-top').fadeOut(400);
            }
        });
        jQuery('#top').on('click', function() {
            jQuery('top').tooltip('hide');
            jQuery('body,html').animate({
                scrollTop: 0
            }, 800);
            return false;
        });

        if(jQuery('.tv-show-back-data').length)
        {
            var url = jQuery('.tv-show-back-data').data('url');
            console.log(url);
            var html = '';
            html += `<div class="tv-single-background">
                <img src="`+url+`">
            </div>`;
            jQuery('#main').prepend(html);
           
        }
    });
})(jQuery);