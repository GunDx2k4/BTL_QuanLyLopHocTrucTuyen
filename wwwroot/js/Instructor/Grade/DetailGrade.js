$(document).ready(function () {

    // Khi ấn "Chấm điểm" → hiển thị form nhập điểm
    $("#btnEditGrade").on("click", function () {
        $("#gradeInfo").hide();
        $("#gradeForm").slideDown(200);
        $(this).hide();
    });

    // Khi ấn "Hủy" → ẩn form, hiển thị lại thông tin
    $("#cancelGrade").on("click", function () {
        $("#gradeForm").slideUp(200);
        $("#gradeInfo").show();
        $("#btnEditGrade").show();
    });

});
