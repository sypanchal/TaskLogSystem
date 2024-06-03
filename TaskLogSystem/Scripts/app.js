const shrink_btn = document.querySelector(".shrink-btn");
const search = document.querySelector(".search");
const dabang_pro_shrink = document.querySelector(".pro-shrink");
const dabang_pro_full = document.querySelector(".pro-full");

/*===== Shrink Sidebar =====*/
shrink_btn.addEventListener("click", () => {
    document.body.classList.toggle("shrink");
    dabang_pro_shrink.classList.toggle("hidden");
    dabang_pro_full.classList.toggle("hidden");
});

/*===== Activate Navlink =====*/
const activate_link = document.querySelectorAll(".nav_link");

function colorLink() {
    if (linkColor) {
    }
}

activate_link.forEach((link) =>
    link.addEventListener("click", function () {
        activate_link.forEach((l) => l.classList.remove("active"));
        this.classList.add("active");
    })
);
