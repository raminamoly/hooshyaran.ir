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

  const getAntiForgeryToken = (form) =>
    form?.querySelector('input[name="__RequestVerificationToken"]')?.value || "";

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
