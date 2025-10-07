// wwwroot/js/site.js
(function () {
    // Safe init params (fall back if the view didn't set window.initialParams)
    const defaults = { lang: 'en-US', seed: '1', likes: 3.5 };
    const init = (typeof window !== 'undefined' && window.initialParams) ? window.initialParams : defaults;

    // Single source of truth
    const state = { ...init, page: 1, mode: 'table' };
    window.app = window.app || {};
    window.app.state = state;

    const $content = $('#content');

    function loadTable(page) {
        state.page = page;
        $.get('/Songs/Table', state, function (html) {
            $content.html(html);                 // REPLACE content (prevents stacking)
            attachTableHandlers();
        });
    }

    function loadGallery(page) {
        state.page = page;
        $.get('/Songs/Gallery', state, function (html) {
            $content.html(html);                 // REPLACE content (no append)
            attachGalleryHandlers();
        });
    }

    // One-and-only reload function
    function reload() {
        state.page = 1;                        // reset page on any param/mode change
        if (state.mode === 'table') loadTable(1);
        else loadGallery(1);
    }

    function attachTableHandlers() {
        // pager (kept)
        $('.page-nav').off('click').on('click', function (e) {
            e.preventDefault();
            const p = parseInt($(this).data('page'), 10);
            loadTable(p);
        });

        // NEW: icon button toggler
        $('.toggle-row').off('click').on('click', function (e) {
            e.stopPropagation();
            const idx = $(this).data('index');
            const $detail = $('#detail-' + idx);
            const expanded = !$detail.hasClass('d-none');

            // collapse all, reset all arrows
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

        // Optional: clicking the row also toggles, but delegates to the button to keep arrows in sync
        $('.row-summary').off('click').on('click', function (e) {
            if (!$(e.target).closest('.toggle-row').length) {
                $(this).find('.toggle-row').trigger('click');
            }
        });
    }

    function attachGalleryHandlers() {
        // Standard pagination (no infinite scroll)
        $('.gal-page').off('click').on('click', function (e) {
            e.preventDefault();
            const p = parseInt($(this).data('page'), 10);
            loadGallery(p);
        });
    }

    // Toolbar bindings (guard + off() before on())
    $(function () {
        if ($('#lang').length) {
            $('#lang').off('change').on('change', function () { state.lang = this.value; reload(); });
        }
        if ($('#seed').length) {
            $('#seed').off('input').on('input', function () {
                state.seed = this.value;
                reload();
            });
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
            $('#modeTable').off('change').on('change', function () {
                if (this.checked) { state.mode = 'table'; reload(); }
            });
        }
        if ($('#modeGallery').length) {
            $('#modeGallery').off('change').on('change', function () {
                if (this.checked) { state.mode = 'gallery'; reload(); }
            });
        }

        // Initial mode: table by default
        loadTable(1);
    });
})();
