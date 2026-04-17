$(document).ready(function () {
    // Diğer genel JS kodları (varsa) buraya eklenebilir.
    // Urunler sayfası Toplu Çıkış modali mantığı Index.cshtml içinde yer almaktadır.
});

function toggleSidebar() {
    $('#sidebar').toggleClass('active');
    $('.sidebar-overlay').toggleClass('active');
}