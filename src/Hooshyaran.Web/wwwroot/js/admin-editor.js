(function () {
  const tagSelects = document.querySelectorAll(".admin-tag-combobox");
  tagSelects.forEach((select) => {
    const wrapper = document.createElement("div");
    wrapper.className = "tag-combobox";

    const trigger = document.createElement("button");
    trigger.type = "button";
    trigger.className = "tag-combobox__trigger";
    trigger.setAttribute("aria-expanded", "false");

    const chips = document.createElement("div");
    chips.className = "tag-combobox__chips";

    const placeholder = document.createElement("span");
    placeholder.className = "tag-combobox__placeholder";
    placeholder.textContent = "انتخاب تگ‌ها";

    const panel = document.createElement("div");
    panel.className = "tag-combobox__panel";
    panel.hidden = true;

    const search = document.createElement("input");
    search.type = "search";
    search.className = "tag-combobox__search";
    search.placeholder = "جستجوی تگ";
    search.setAttribute("aria-label", "جستجوی تگ");

    const list = document.createElement("div");
    list.className = "tag-combobox__list";

    const options = Array.from(select.options).map((option) => ({
      value: option.value,
      text: option.text,
      selected: option.selected
    }));

    const syncNativeSelect = () => {
      Array.from(select.options).forEach((option) => {
        option.selected = options.some((item) => item.value === option.value && item.selected);
      });
    };

    const closePanel = () => {
      panel.hidden = true;
      trigger.setAttribute("aria-expanded", "false");
    };

    const openPanel = () => {
      panel.hidden = false;
      trigger.setAttribute("aria-expanded", "true");
      search.focus();
    };

    const renderChips = () => {
      chips.innerHTML = "";
      const selectedOptions = options.filter((item) => item.selected);
      placeholder.hidden = selectedOptions.length > 0;
      selectedOptions.forEach((item) => {
        const chip = document.createElement("span");
        chip.className = "tag-combobox__chip";
        chip.textContent = item.text;

        const remove = document.createElement("button");
        remove.type = "button";
        remove.className = "tag-combobox__remove";
        remove.setAttribute("aria-label", `حذف ${item.text}`);
        remove.textContent = "×";
        remove.addEventListener("click", (event) => {
          event.stopPropagation();
          item.selected = false;
          syncNativeSelect();
          render();
        });

        chip.appendChild(remove);
        chips.appendChild(chip);
      });
    };

    const renderList = () => {
      const query = search.value.trim().toLowerCase();
      list.innerHTML = "";
      options
        .filter((item) => item.text.toLowerCase().includes(query))
        .forEach((item) => {
          const optionButton = document.createElement("button");
          optionButton.type = "button";
          optionButton.className = "tag-combobox__option";
          optionButton.setAttribute("aria-pressed", item.selected ? "true" : "false");
          optionButton.textContent = item.text;
          optionButton.addEventListener("click", () => {
            item.selected = !item.selected;
            syncNativeSelect();
            render();
            closePanel();
          });
          list.appendChild(optionButton);
        });

      if (!list.children.length) {
        const empty = document.createElement("span");
        empty.className = "tag-combobox__empty";
        empty.textContent = "تگی پیدا نشد.";
        list.appendChild(empty);
      }
    };

    const render = () => {
      renderChips();
      renderList();
    };

    trigger.addEventListener("click", () => {
      if (panel.hidden) {
        openPanel();
      } else {
        closePanel();
      }
    });

    search.addEventListener("input", renderList);
    search.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        closePanel();
        trigger.focus();
      }
    });

    document.addEventListener("click", (event) => {
      if (!wrapper.contains(event.target)) {
        closePanel();
      }
    });

    select.classList.add("tag-combobox__native");
    select.before(wrapper);
    trigger.append(placeholder, chips);
    panel.append(search, list);
    wrapper.append(trigger, panel, select);
    render();
  });

  const persianDatePickers = document.querySelectorAll(".js-persian-datetime-picker");
  const toEnglishDigits = (value) => (value || "")
    .replace(/[۰-۹]/g, (digit) => "۰۱۲۳۴۵۶۷۸۹".indexOf(digit).toString())
    .replace(/[٠-٩]/g, (digit) => "٠١٢٣٤٥٦٧٨٩".indexOf(digit).toString());
  const toPersianDigits = (value) => String(value).replace(/\d/g, (digit) => "۰۱۲۳۴۵۶۷۸۹"[Number(digit)]);
  const pad2 = (value) => String(value).padStart(2, "0");
  const getPersianMonthDays = (year, month) => {
    if (month <= 6) {
      return 31;
    }

    if (month <= 11) {
      return 30;
    }

    return [1, 5, 9, 13, 17, 22, 26, 30].includes(year % 33) ? 30 : 29;
  };

  const parsePersianDateTimeValue = (value) => {
    const normalized = toEnglishDigits(value).replace(/-/g, "/").trim();
    const match = normalized.match(/^(\d{4})\/(\d{1,2})\/(\d{1,2})(?:\s+(\d{1,2}):(\d{1,2}))?$/);
    if (!match) {
      return null;
    }

    return {
      year: Number(match[1]),
      month: Number(match[2]),
      day: Number(match[3]),
      hour: Number(match[4] || 0),
      minute: Number(match[5] || 0)
    };
  };

  const fillNumberSelect = (select, start, end, selected, formatter = (value) => value) => {
    select.innerHTML = "";
    for (let value = start; value <= end; value += 1) {
      const option = document.createElement("option");
      option.value = String(value);
      option.textContent = formatter(value);
      option.selected = value === selected;
      select.appendChild(option);
    }
  };

  persianDatePickers.forEach((picker) => {
    const valueInput = picker.querySelector(".js-persian-datetime-value");
    const yearSelect = picker.querySelector('[data-persian-part="year"]');
    const monthSelect = picker.querySelector('[data-persian-part="month"]');
    const daySelect = picker.querySelector('[data-persian-part="day"]');
    const hourSelect = picker.querySelector('[data-persian-part="hour"]');
    const minuteSelect = picker.querySelector('[data-persian-part="minute"]');
    if (!valueInput || !yearSelect || !monthSelect || !daySelect || !hourSelect || !minuteSelect) {
      return;
    }

    const parsed = parsePersianDateTimeValue(valueInput.value) || {
      year: Number(picker.dataset.startYear || 1400),
      month: 1,
      day: 1,
      hour: 0,
      minute: 0
    };
    const startYear = Number(picker.dataset.startYear || parsed.year - 5);
    const endYear = Number(picker.dataset.endYear || parsed.year + 5);

    const syncValue = () => {
      valueInput.value = `${yearSelect.value}/${pad2(monthSelect.value)}/${pad2(daySelect.value)} ${pad2(hourSelect.value)}:${pad2(minuteSelect.value)}`;
      valueInput.dispatchEvent(new Event("input", { bubbles: true }));
      valueInput.dispatchEvent(new Event("change", { bubbles: true }));
    };

    const fillDays = () => {
      const year = Number(yearSelect.value);
      const month = Number(monthSelect.value);
      const selectedDay = Math.min(Number(daySelect.value || parsed.day), getPersianMonthDays(year, month));
      fillNumberSelect(daySelect, 1, getPersianMonthDays(year, month), selectedDay, (value) => toPersianDigits(pad2(value)));
    };

    fillNumberSelect(yearSelect, startYear, endYear, parsed.year, (value) => toPersianDigits(value));
    fillNumberSelect(monthSelect, 1, 12, parsed.month, (value) => toPersianDigits(pad2(value)));
    fillDays();
    fillNumberSelect(hourSelect, 0, 23, parsed.hour, (value) => toPersianDigits(pad2(value)));
    fillNumberSelect(minuteSelect, 0, 59, parsed.minute, (value) => toPersianDigits(pad2(value)));
    syncValue();

    yearSelect.addEventListener("change", () => {
      fillDays();
      syncValue();
    });
    monthSelect.addEventListener("change", () => {
      fillDays();
      syncValue();
    });
    [daySelect, hourSelect, minuteSelect].forEach((select) => {
      select.addEventListener("change", syncValue);
    });
    valueInput.form?.addEventListener("submit", syncValue);
  });

  const imageExtensionPattern = /\.(jpg|jpeg|png|webp|gif)(\?.*)?$/i;
  const mediaPicker = document.querySelector("[data-media-picker]");
  const mediaPickerButtons = document.querySelectorAll(".js-media-picker");
  const mediaInputs = document.querySelectorAll(".js-media-input");
  const copyMediaButtons = document.querySelectorAll(".js-copy-media-url");
  let activeMediaInput = null;
  let mediaSearchTimer = null;

  const getAntiForgeryToken = (form) =>
    form?.querySelector('input[name="__RequestVerificationToken"]')?.value || "";

  const isImageUrl = (url) => imageExtensionPattern.test(url || "");

  const updateMediaPreview = (input) => {
    const preview = input.closest(".media-field")?.querySelector(".js-media-preview");
    if (!preview) {
      return;
    }

    const url = input.value.trim();
    preview.innerHTML = "";
    if (url && isImageUrl(url)) {
      const image = document.createElement("img");
      image.src = url;
      image.alt = "پیش‌نمایش فایل انتخاب‌شده";
      image.loading = "lazy";
      preview.appendChild(image);
      return;
    }

    const empty = document.createElement("span");
    empty.textContent = url ? "این فایل پیش‌نمایش تصویری ندارد." : "تصویری انتخاب نشده است.";
    preview.appendChild(empty);
  };

  mediaInputs.forEach((input) => {
    updateMediaPreview(input);
    input.addEventListener("input", () => updateMediaPreview(input));
    input.addEventListener("change", () => updateMediaPreview(input));
  });

  copyMediaButtons.forEach((button) => {
    button.addEventListener("click", async () => {
      const url = button.dataset.mediaUrl || "";
      if (!url) {
        return;
      }

      try {
        await navigator.clipboard.writeText(url);
        button.title = "کپی شد";
        window.setTimeout(() => {
          button.title = "کپی مسیر";
        }, 1600);
      } catch {
        window.prompt("مسیر فایل:", url);
      }
    });
  });

  const mediaDetail = document.querySelector("[data-media-detail]");
  if (mediaDetail) {
    const preview = mediaDetail.querySelector("[data-media-detail-preview]");
    const title = mediaDetail.querySelector("[data-media-detail-name]");
    const url = mediaDetail.querySelector("[data-media-detail-url]");
    const extension = mediaDetail.querySelector("[data-media-detail-extension]");
    const contentType = mediaDetail.querySelector("[data-media-detail-content-type]");
    const size = mediaDetail.querySelector("[data-media-detail-size]");
    const date = mediaDetail.querySelector("[data-media-detail-date]");
    const used = mediaDetail.querySelector("[data-media-detail-used]");
    const usage = mediaDetail.querySelector("[data-media-detail-usage]");
    const copyButton = mediaDetail.querySelector("[data-media-detail-copy]");
    const inputs = {
      url: mediaDetail.querySelector('[data-media-detail-input="url"]'),
      alt: mediaDetail.querySelector('[data-media-detail-input="alt"]'),
      title: mediaDetail.querySelector('[data-media-detail-input="title"]'),
      description: mediaDetail.querySelector('[data-media-detail-input="description"]'),
      seoDescription: mediaDetail.querySelector('[data-media-detail-input="seoDescription"]')
    };

    const closeDetail = () => {
      mediaDetail.hidden = true;
      mediaDetail.setAttribute("aria-hidden", "true");
    };

    const openDetail = (card) => {
      const data = card.dataset;
      title.textContent = data.name || "";
      url.textContent = data.url || "";
      extension.textContent = data.extension || "";
      contentType.textContent = data.contentType || "";
      size.textContent = data.size || "";
      date.textContent = data.date || "";
      used.textContent = data.used === "true" ? "استفاده‌شده" : "استفاده‌نشده";
      usage.textContent = data.usage || "در مقاله، محصول یا صفحه‌ای پیدا نشد.";
      copyButton.dataset.mediaUrl = data.url || "";
      inputs.url.value = data.url || "";
      inputs.alt.value = data.alt || "";
      inputs.title.value = data.title || "";
      inputs.description.value = data.description || "";
      inputs.seoDescription.value = data.seoDescription || "";

      preview.innerHTML = "";
      if (data.isImage === "true") {
        const image = document.createElement("img");
        image.src = data.url || "";
        image.alt = data.alt || data.name || "";
        image.loading = "lazy";
        preview.appendChild(image);
      } else {
        const fileTile = document.createElement("span");
        fileTile.className = "media-file-tile media-file-tile--large";
        fileTile.dataset.fileExtension = data.extension || "FILE";

        const ext = document.createElement("span");
        ext.textContent = data.extension || "FILE";

        const label = document.createElement("small");
        label.textContent = data.contentType || "فایل";

        fileTile.append(ext, label);
        preview.appendChild(fileTile);
      }

      mediaDetail.hidden = false;
      mediaDetail.setAttribute("aria-hidden", "false");
      inputs.alt.focus();
    };

    document.querySelectorAll("[data-media-card]").forEach((card) => {
      card.querySelectorAll("[data-media-open]").forEach((button) => {
        button.addEventListener("click", () => openDetail(card));
      });
    });

    mediaDetail.querySelectorAll("[data-media-detail-close]").forEach((button) => {
      button.addEventListener("click", closeDetail);
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && !mediaDetail.hidden) {
        closeDetail();
      }
    });
  }

  if (mediaPicker && mediaPickerButtons.length) {
    const grid = mediaPicker.querySelector("[data-media-picker-grid]");
    const search = mediaPicker.querySelector("[data-media-picker-search]");
    const status = mediaPicker.querySelector("[data-media-picker-status]");
    const uploadInput = mediaPicker.querySelector("[data-media-picker-upload]");
    const uploadButton = mediaPicker.querySelector("[data-media-picker-upload-button]");

    const closePicker = () => {
      mediaPicker.hidden = true;
      mediaPicker.setAttribute("aria-hidden", "true");
      activeMediaInput = null;
    };

    const setPickerStatus = (message) => {
      status.textContent = message || "";
    };

    const normalizeMediaFile = (file) => ({
      name: file.name || file.Name || "",
      url: file.url || file.Url || "",
      extension: file.extension || file.Extension || "",
      size: file.size || file.Size || "",
      lastModified: file.lastModified || file.LastModified || "",
      isImage: Boolean(file.isImage ?? file.IsImage)
    });

    const renderMediaFiles = (files) => {
      grid.innerHTML = "";
      if (!files.length) {
        const empty = document.createElement("div");
        empty.className = "media-picker__empty";
        empty.textContent = "فایلی پیدا نشد.";
        grid.appendChild(empty);
        return;
      }

      files.map(normalizeMediaFile).forEach((file) => {
        const card = document.createElement("button");
        card.type = "button";
        card.className = "media-picker-card";
        card.title = file.url;

        const preview = document.createElement("span");
        preview.className = "media-picker-card__preview";
        if (file.isImage) {
          const image = document.createElement("img");
          image.src = file.url;
          image.alt = file.name;
          image.loading = "lazy";
          preview.appendChild(image);
        } else {
          preview.textContent = file.extension || "FILE";
        }

        const name = document.createElement("strong");
        name.textContent = file.name;

        const meta = document.createElement("small");
        meta.textContent = file.size;

        card.append(preview, name, meta);
        card.addEventListener("click", () => {
          if (!activeMediaInput) {
            return;
          }

          activeMediaInput.value = file.url;
          activeMediaInput.dispatchEvent(new Event("input", { bubbles: true }));
          activeMediaInput.dispatchEvent(new Event("change", { bubbles: true }));
          closePicker();
        });
        grid.appendChild(card);
      });
    };

    const loadMediaFiles = async () => {
      const params = new URLSearchParams({
        handler: "Library",
        fileType: "images",
        searchTerm: search.value.trim()
      });
      setPickerStatus("در حال دریافت فایل‌ها...");

      try {
        const response = await fetch(`/admin/media?${params.toString()}`, {
          headers: { Accept: "application/json" }
        });
        const files = await response.json();
        if (!response.ok) {
          throw new Error(files.error || "لیست فایل‌ها دریافت نشد.");
        }

        renderMediaFiles(files);
        setPickerStatus("");
      } catch (error) {
        renderMediaFiles([]);
        setPickerStatus(error.message);
      }
    };

    const openPicker = (input) => {
      activeMediaInput = input;
      mediaPicker.hidden = false;
      mediaPicker.setAttribute("aria-hidden", "false");
      search.value = "";
      loadMediaFiles();
      search.focus();
    };

    mediaPickerButtons.forEach((button) => {
      button.addEventListener("click", () => {
        const targetSelector = button.dataset.mediaTarget || "";
        if (!targetSelector) {
          return;
        }

        const input = document.querySelector(targetSelector);
        if (input) {
          openPicker(input);
        }
      });
    });

    mediaPicker.querySelectorAll("[data-media-picker-close]").forEach((button) => {
      button.addEventListener("click", closePicker);
    });

    search.addEventListener("input", () => {
      window.clearTimeout(mediaSearchTimer);
      mediaSearchTimer = window.setTimeout(loadMediaFiles, 250);
    });

    uploadButton.addEventListener("click", () => uploadInput.click());
    uploadInput.addEventListener("change", async () => {
      const file = uploadInput.files?.[0];
      if (!file || !activeMediaInput) {
        return;
      }

      const formData = new FormData();
      formData.append("file", file);
      setPickerStatus("در حال آپلود فایل...");

      try {
        const response = await fetch("/admin/media?handler=UploadJson", {
          method: "POST",
          headers: {
            RequestVerificationToken: getAntiForgeryToken(activeMediaInput.form)
          },
          body: formData
        });
        const payload = await response.json();
        if (!response.ok) {
          throw new Error(payload.error || "آپلود انجام نشد.");
        }

        activeMediaInput.value = normalizeMediaFile(payload).url;
        activeMediaInput.dispatchEvent(new Event("input", { bubbles: true }));
        activeMediaInput.dispatchEvent(new Event("change", { bubbles: true }));
        await loadMediaFiles();
        setPickerStatus("فایل آپلود و انتخاب شد.");
      } catch (error) {
        setPickerStatus(error.message);
      } finally {
        uploadInput.value = "";
      }
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && !mediaPicker.hidden) {
        closePicker();
      }
    });
  }

  const editorTargets = document.querySelectorAll(".js-rich-editor");
  if (!editorTargets.length) {
    return;
  }

  const commands = [
    { command: "undo", label: "↶", title: "بازگشت" },
    { command: "redo", label: "↷", title: "جلو رفتن" },
    { command: "formatBlock", value: "h2", label: "H2", title: "تیتر ۲" },
    { command: "formatBlock", value: "h3", label: "H3", title: "تیتر ۳" },
    { command: "formatBlock", value: "p", label: "P", title: "پاراگراف" },
    { command: "bold", label: "B", title: "Bold" },
    { command: "italic", label: "I", title: "Italic" },
    { command: "insertUnorderedList", label: "•", title: "لیست نقطه‌ای" },
    { command: "insertOrderedList", label: "1.", title: "لیست شماره‌ای" },
    { command: "formatBlock", value: "blockquote", label: "❝", title: "نقل قول" },
    { action: "link", label: "Link", title: "افزودن لینک" },
    { action: "image", label: "Img", title: "درج عکس" },
    { action: "table", label: "Table", title: "درج جدول" },
    { command: "removeFormat", label: "پاک‌سازی", title: "پاک کردن فرمت" },
    { action: "source", label: "</>", title: "نمایش/ویرایش HTML" }
  ];

  const escapeHtml = (value) => value
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");

  const toHtml = (value) => {
    if (value.includes("<") && value.includes(">")) {
      return value;
    }

    return value
      .split(/\n{2,}/)
      .map((block) => block.trim())
      .filter(Boolean)
      .map((block) => `<p>${escapeHtml(block).replace(/\n/g, "<br>")}</p>`)
      .join("");
  };

  const normalizeHtml = (value) => value
    .replace(/<script[\s\S]*?>[\s\S]*?<\/script>/gi, "")
    .replace(/\son\w+="[^"]*"/gi, "")
    .replace(/\son\w+='[^']*'/gi, "")
    .replace(/javascript:/gi, "");

  const insertHtml = (surface, html) => {
    surface.focus();
    document.execCommand("insertHTML", false, html);
  };

  editorTargets.forEach((textarea) => {
    const wrapper = document.createElement("div");
    wrapper.className = "rich-editor";

    const toolbar = document.createElement("div");
    toolbar.className = "rich-editor__toolbar";
    toolbar.setAttribute("role", "toolbar");
    toolbar.setAttribute("aria-label", "ابزارهای ویرایش متن");

    const surface = document.createElement("div");
    surface.className = "rich-editor__surface";
    surface.contentEditable = "true";
    surface.dir = "rtl";
    surface.innerHTML = toHtml(textarea.value);
    surface.setAttribute("aria-label", textarea.labels?.[0]?.innerText || "ویرایشگر متن");

    const source = document.createElement("textarea");
    source.className = "rich-editor__html";
    source.dir = "ltr";
    source.spellcheck = false;
    source.hidden = true;
    source.value = surface.innerHTML.trim();

    const status = document.createElement("div");
    status.className = "rich-editor__status";
    status.setAttribute("aria-live", "polite");

    const uploadInput = document.createElement("input");
    uploadInput.type = "file";
    uploadInput.accept = ".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp";
    uploadInput.hidden = true;

    let sourceMode = false;

    const syncFromSurface = () => {
      textarea.value = normalizeHtml(surface.innerHTML.trim());
      source.value = textarea.value;
    };

    const syncFromSource = () => {
      textarea.value = normalizeHtml(source.value.trim());
      surface.innerHTML = textarea.value;
    };

    const toggleSourceMode = () => {
      sourceMode = !sourceMode;
      if (sourceMode) {
        syncFromSurface();
        source.hidden = false;
        surface.hidden = true;
        source.focus();
      } else {
        syncFromSource();
        source.hidden = true;
        surface.hidden = false;
        surface.focus();
      }
    };

    const addLink = () => {
      const url = window.prompt("آدرس لینک را وارد کنید:", "https://");
      if (!url) {
        return;
      }

      document.execCommand("createLink", false, url);
      const links = surface.querySelectorAll(`a[href="${CSS.escape(url)}"]`);
      links.forEach((link) => {
        if (url.startsWith("http")) {
          link.target = "_blank";
          link.rel = "noopener noreferrer";
        }
      });
      syncFromSurface();
    };

    const addImageByUrl = () => {
      const url = window.prompt("آدرس تصویر را وارد کنید:", "/uploads/editor/");
      if (!url) {
        return;
      }

      const alt = window.prompt("متن جایگزین تصویر:", "") || "";
      insertHtml(surface, `<figure><img src="${escapeHtml(url)}" alt="${escapeHtml(alt)}"><figcaption>${escapeHtml(alt)}</figcaption></figure>`);
      syncFromSurface();
    };

    const insertTable = () => {
      insertHtml(surface, "<table><tbody><tr><th>عنوان</th><th>عنوان</th></tr><tr><td>مقدار</td><td>مقدار</td></tr></tbody></table>");
      syncFromSurface();
    };

    uploadInput.addEventListener("change", async () => {
      const file = uploadInput.files?.[0];
      if (!file) {
        return;
      }

      status.textContent = "در حال آپلود تصویر...";
      const formData = new FormData();
      formData.append("file", file);

      try {
        const response = await fetch("/admin/editor/upload", {
          method: "POST",
          headers: {
            RequestVerificationToken: getAntiForgeryToken(textarea.form)
          },
          body: formData
        });
        const payload = await response.json();
        if (!response.ok) {
          throw new Error(payload.error || "آپلود انجام نشد.");
        }

        const alt = window.prompt("متن جایگزین تصویر:", file.name.replace(/\.[^.]+$/, "")) || "";
        insertHtml(surface, `<figure><img src="${escapeHtml(payload.url)}" alt="${escapeHtml(alt)}"><figcaption>${escapeHtml(alt)}</figcaption></figure>`);
        status.textContent = "تصویر اضافه شد.";
        syncFromSurface();
      } catch (error) {
        status.textContent = error.message;
      } finally {
        uploadInput.value = "";
      }
    });

    commands.forEach((item) => {
      const button = document.createElement("button");
      button.type = "button";
      button.textContent = item.label;
      button.title = item.title || item.label;
      button.addEventListener("click", () => {
        if (item.action === "source") {
          toggleSourceMode();
          return;
        }

        if (sourceMode) {
          toggleSourceMode();
        }

        surface.focus();
        if (item.action === "link") {
          addLink();
        } else if (item.action === "image") {
          const choice = window.confirm("برای آپلود تصویر OK را بزنید. برای درج با URL گزینه Cancel را بزنید.");
          if (choice) {
            uploadInput.click();
          } else {
            addImageByUrl();
          }
        } else if (item.action === "table") {
          insertTable();
        } else {
          document.execCommand(item.command, false, item.value || null);
          syncFromSurface();
        }
      });
      toolbar.appendChild(button);
    });

    surface.addEventListener("input", () => {
      syncFromSurface();
    });

    source.addEventListener("input", () => {
      textarea.value = normalizeHtml(source.value.trim());
    });

    textarea.classList.add("rich-editor__source");
    textarea.before(wrapper);
    wrapper.append(toolbar, surface, source, status, uploadInput, textarea);

    textarea.form?.addEventListener("submit", () => {
      if (sourceMode) {
        syncFromSource();
      } else {
        syncFromSurface();
      }
    });
  });
})();
