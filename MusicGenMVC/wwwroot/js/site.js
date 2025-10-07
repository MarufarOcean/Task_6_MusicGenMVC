(function () {
    const defaults = { lang: 'en-US', seed: '1', likes: 3.5 };
    const init = (typeof window !== 'undefined' && window.initialParams) ? window.initialParams : defaults;
    const state = { ...init, page: 1, mode: 'table', loading: false, totalPages: null };
    window.app = window.app || {};
    window.app.state = state;

    const $content = $('#content');

    function loadTable(page) {
        state.page = page;
        $.get('/Songs/Table', state, function (html) {
            $content.html(html);
            attachTableHandlers();
        });
    }

    function renderGallery(html, append) {
        if (!append) {
            $content.html(html);
            const $root = $('#gallery');
            state.totalPages = parseInt($root.data('total-pages'), 10);
            state.page = parseInt($root.data('current-page'), 10);
            attachGalleryHandlers();
        } else {
            const $tmp = $('<div>').html(html);
            const $items = $tmp.find('.gal-items').children();
            $('.gal-items').append($items);
        }
    }

    function loadGallery(page, append = false) {
        state.page = page;
        state.loading = true;
        $.get('/Songs/Gallery', state, function (html) {
            renderGallery(html, append);
        }).always(function () {
            state.loading = false;
            updateGalleryStatus();
        });
    }

    function updateGalleryStatus() {
        const atEnd = state.totalPages !== null && state.page >= state.totalPages;
        $('#gal-status').text(atEnd ? '' : 'Loading more on scroll…');
    }

    function reload() {
        state.page = 1;
        if (state.mode === 'table') loadTable(1);
        else loadGallery(1, false);
    }

    function attachTableHandlers() {
        $('.page-nav').off('click').on('click', function (e) {
            e.preventDefault();
            const p = parseInt($(this).data('page'), 10);
            loadTable(p);
        });

        $('.toggle-row').off('click').on('click', function (e) {
            e.stopPropagation();
            const idx = $(this).data('index');
            const $detail = $('#detail-' + idx);
            const expanded = !$detail.hasClass('d-none');
            $('tr.detail').addClass('d-none');
            $('.toggle-row .arrow').text('▼');
            $('.toggle-row').attr('aria-expanded', 'false');
            if (!expanded) {
                $(this).find('.arrow').text('▲');
                $(this).attr('aria-expanded', 'true');
                $detail.removeClass('d-none').html('<td colspan="7">Loading...</td>');
                $.get('/Songs/Detail', { ...state, index: idx }, function (html) {
                    $detail.html('<td colspan="7">' + html + '</td>');
                });
            }
        });

        $('.row-summary').off('click').on('click', function (e) {
            if (!$(e.target).closest('.toggle-row').length) {
                $(this).find('.toggle-row').trigger('click');
            }
        });
    }

    function attachGalleryHandlers() {
        $(window).off('scroll.gallery').on('scroll.gallery', function () {
            if (state.loading) return;
            const nearBottom = $(window).scrollTop() + $(window).height() >= ($(document).height() - 200);
            const more = state.totalPages === null || state.page < state.totalPages;
            if (nearBottom && more) {
                loadGallery(state.page + 1, true);
            }
        });
        updateGalleryStatus();
    }

    $(function () {
        if ($('#lang').length) {
            $('#lang').off('change').on('change', function () { state.lang = this.value; reload(); });
        }
        if ($('#seed').length) {
            $('#seed').off('input').on('input', function () { state.seed = this.value; reload(); });
            $('#randomSeed').off('click').on('click', function () {
                const rnd = Math.floor(Math.random() * Number.MAX_SAFE_INTEGER);
                $('#seed').val(String(rnd));
                state.seed = String(rnd);
                reload();
            });
        }
        if ($('#likes').length) {
            $('#likes').off('input').on('input', function () { state.likes = this.value; reload(); });
        }
        if ($('#modeTable').length) {
            $('#modeTable').off('change').on('change', function () { if (this.checked) { state.mode = 'table'; reload(); } });
        }
        if ($('#modeGallery').length) {
            $('#modeGallery').off('change').on('change', function () { if (this.checked) { state.mode = 'gallery'; reload(); } });
        }

        loadTable(1);
    });
})();
