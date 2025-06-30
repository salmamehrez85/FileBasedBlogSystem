document.addEventListener("DOMContentLoaded", () => {
  const loginContainer = document.getElementById("loginContainer");
  const registerContainer = document.getElementById("registerContainer");
  const showRegister = document.getElementById("showRegister");
  const showLogin = document.getElementById("showLogin");
  const loginForm = document.getElementById("loginForm");
  const registerForm = document.getElementById("registerForm");
  const errorMessage = document.getElementById("errorMessage");
  const popupMessage = document.getElementById("popupMessage");

  showRegister.addEventListener("click", (e) => {
    e.preventDefault();
    loginContainer.style.display = "none";
    registerContainer.style.display = "block";
  });

  showLogin.addEventListener("click", (e) => {
    e.preventDefault();
    registerContainer.style.display = "none";
    loginContainer.style.display = "block";
  });

  loginForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    const Username = document.getElementById("loginUsername").value;
    const Password = document.getElementById("loginPassword").value;

    try {
      const response = await fetch("/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Username, Password }),
      });

      if (response.ok) {
        const { token } = await response.json();
        localStorage.setItem("jwt_token", token);
        showPopup("Signed in successfully! Redirecting to home page...");
        setTimeout(() => {
          window.location.href = "/";
        }, 5000);
      } else {
        let error = await response.text();
        try {
          const errObj = JSON.parse(error);
          error = errObj.title || errObj.detail || error;
        } catch {}
        displayError(`Sign in failed: ${error}`);
      }
    } catch (err) {
      displayError(`An error occurred: ${err.message}`);
    }
  });

  registerForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    const Username = document.getElementById("registerUsername").value;
    const Email = document.getElementById("registerEmail").value;
    const Password = document.getElementById("registerPassword").value;
    const Roles = ["Author"];

    try {
      const response = await fetch("/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Username, Email, Password, Roles }),
      });

      if (response.ok) {
        showPopup("Signed up successfully! Please sign in.");
        setTimeout(() => {
          window.location.reload();
        }, 10000);
      } else {
        let error = await response.text();
        try {
          const errObj = JSON.parse(error);
          error = errObj.title || errObj.detail || error;
        } catch {}
        displayError(`Sign up failed: ${error}`);
      }
    } catch (err) {
      displayError(`An error occurred: ${err.message}`);
    }
  });

  function displayError(message) {
    errorMessage.textContent = message;
    errorMessage.style.display = "block";
  }

  function showPopup(message) {
    popupMessage.textContent = message;
    popupMessage.style.display = "block";
    setTimeout(() => {
      popupMessage.style.display = "none";
    }, 10000);
  }
});
