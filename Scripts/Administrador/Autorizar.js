$(document).ready(function () {

    $('.btn-cerrar-administrar').click(function () {
        $('.modalValidacion-footer-autorizado').hide();
        $('.modalValidacion-footer-pendiente').hide();
        $('.modalValidacion-footer-denegado').hide();
    });

    $('.btn-administrar').click(function () {
        // Obtén los datos del docente desde los atributos data-*
        const docenteId = $(this).data('docente-id');
        const apellidoPaterno = $(this).data('docente-apellidopaterno');
        const apellidoMaterno = $(this).data('docente-apellidomaterno');
        const nombres = $(this).data('docente-nombres');
        const email = $(this).data('docente-email');
        const estatus = $(this).data('docente-estatus');
        const envioCorreo = $(this).data('docente-enviocorreo');

        $('#modalApellidoPaterno').text(apellidoPaterno);
        $('#modalApellidoMaterno').text(apellidoMaterno);
        $('#modalNombres').text(nombres);
        $('#modalEmail').text(email);
        $('#modalEstatus').text(estatus);
        $('#abrirConfirmacionModal').attr('data-docenteid', docenteId);
        $('#abrirDenegacionModal').attr('data-docenteid', docenteId);
        $('#abrirReenviarModal').attr('data-docenteid', docenteId);

        let badgeClass = '';
        switch (estatus) {
            case 'Autorizado':
                console.log('AUTORIZADO');
                badgeClass = 'bg-success';
                $('.modalValidacion-footer-autorizado').show();
                break;
            case 'Denegado':
                badgeClass = 'bg-danger';
                $('.modalValidacion-footer-denegado').show();
                break;
            case 'Pendiente':
                if (envioCorreo == "Enviado") {
                    badgeClass = 'bg-warning';
                    $('.modalValidacion-footer-autorizado').show();
                } else {
                    badgeClass = 'bg-warning';
                    $('.modalValidacion-footer-pendiente').show();
                }
                break;
        }
        $('#modalEstatus').html(`<span class="badge ${badgeClass}">${estatus}</span>`);
    });






    $('#abrirConfirmacionModal').click(function () {
        var docenteId = $(this).data('docenteid');

        $('#confirmarAutorizarBtn').attr('data-confirmar-autorizacionbtn-docenteid', docenteId);
        $('#confirmacionModal').modal('show');
    });




    $('#abrirDenegacionModal').click(function () {
        var docenteId = $(this).data('docenteid');
        $('#confirmarDenegarBtn').attr('data-denegar-autorizacionbtn-docenteid', docenteId);
        $('#denegacionModal').modal('show');
    });

    $('#abrirReenviarModal').click(function () {
        var docenteId = $(this).data('docenteid');
        $('#confirmarReenviarBtn').attr('data-confirmar-reenviarbtn-docenteid', docenteId);
        $('#reenviarModal').modal('show');
    });




    $('#confirmarAutorizarBtn').click(function () {
        var id = $(this).data('confirmar-autorizacionbtn-docenteid');
        let token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: `/Administrador/AutorizarDocente`,
            type: 'POST',
            //headers: {
            //    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            //},
            //data: {
            //    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            //    docenteId:id
            //},
            data: {
                __RequestVerificationToken: token,
                docenteId: id // tu variable con el ID del docente
            },
            //contentType: 'application/json',
            success: function (response) {
                $('#confirmacionModal').modal('hide');
                $('#modalValidacion').modal('hide');
                location.reload();
            },
            error: function (status, error) {
                $('#confirmacionModal').modal('hide');
                $('#modalValidacion').modal('hide');
                alert('No se pudo autorizar el docente');
            }
        });
    });




    $('#confirmarDenegarBtn').click(function () {
        var id = $(this).data('denegar-autorizacionbtn-docenteid');

        $.ajax({
            url: '/Administrador/DenegarDocente',
            type: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify(id),
            contentType: 'application/json',
            success: function (response) {
                $('#denegacionModal').modal('hide');
                $('#modalValidacion').modal('hide');
                location.reload();
            },
            error: function (status, error) {
                $('#denegacionModal').modal('hide');
                $('#modalValidacion').modal('hide');
                location.reload();
            }
        });

    });



    $('#confirmarReenviarBtn').click(function () {
        var id = $(this).data('confirmar-reenviarbtn-docenteid');
        $.ajax({
            url: '/Administrador/ReenviarCodigo',
            type: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify(id),
            contentType: 'application/json',
            success: function (response) {
                $('#reenviarModal').modal('hide');
                $('#modalValidacion').modal('hide');
                location.reload();
            },
            error: function (status, error) {
                $('#reenviarModal').modal('hide');
                $('#modalValidacion').modal('hide');
                location.reload();
            }
        });


    });

});