const POSTS_PER_PAGE = 2;
let currentPage = 1;
let category = "";
let allPosts = [];

const postsContainer = document.getElementById("postsContainer");
const paginationContainer = document.getElementById("pagination");
const categoryTitle = document.getElementById("categoryTitle");
const searchInput = document.getElementById("searchInput");
const searchButton = document.getElementById("searchButton");

function setupSearchEvents() {
  searchButton.addEventListener("click", handleSearch);
  searchInput.addEventListener("keypress", (e) => {
    if (e.key === "Enter") handleSearch();
  });
}

function handleSearch() {
  const enteredCategory = searchInput.value.trim().toLowerCase();
  if (!enteredCategory) return;

  category = enteredCategory;
  currentPage = 1;
  categoryTitle.textContent = `Category: ${category}`;
  fetchAndRenderCategoryPosts();
}

async function fetchAndRenderCategoryPosts() {
  try {
    const response = await fetch(
      `/posts/category/${encodeURIComponent(category)}`
    );
    if (!response.ok) throw new Error("Failed to fetch category posts");

    const data = await response.json();
    allPosts = Array.isArray(data) ? data : [];

    renderPage(currentPage);
  } catch (err) {
    console.error(err);
    postsContainer.innerHTML = `<p>Error loading posts.</p>`;
    paginationContainer.innerHTML = "";
  }
}

function renderPage(page) {
  const totalPosts = allPosts.length;
  const totalPages = Math.ceil(totalPosts / POSTS_PER_PAGE);
  currentPage = Math.min(page, totalPages);

  const startIndex = (currentPage - 1) * POSTS_PER_PAGE;
  const paginatedPosts = allPosts.slice(
    startIndex,
    startIndex + POSTS_PER_PAGE
  );

  displayPosts(paginatedPosts);
  displayPagination(totalPages);
}

function displayPosts(posts) {
  postsContainer.innerHTML = posts.length
    ? posts
        .map(
          (post) => `
        <article class="post-card">
          <div class="post-content">
            <h2 class="post-title">
              <a href="/posts/${
                post.slug
              }" style="text-decoration: none; color: inherit;">
                ${post.title || "Untitled"}
              </a>
            </h2>
            <p class="post-excerpt">${
              post.description || "No description available"
            }</p>
            <div class="post-meta">
              <span class="post-date">
                <i class="fas fa-calendar"></i>
                ${
                  post.publishedDate
                    ? new Date(post.publishedDate).toLocaleDateString()
                    : "No date"
                }
              </span>
              <div class="post-author">
                <i class="fas fa-user"></i> By ${post.author || "Unknown"}
              </div>
            </div>
          </div>
        </article>
      `
        )
        .join("")
    : `<p>No posts found for category "${category}".</p>`;
}

function displayPagination(totalPages) {
  if (totalPages <= 1) {
    paginationContainer.innerHTML = "";
    return;
  }

  const buttons = [];

  buttons.push(`
    <button class="pagination-btn" onclick="goToPage(${currentPage - 1})"
      ${currentPage === 1 ? "disabled" : ""}>
      Prev
    </button>
  `);

  for (let i = 1; i <= totalPages; i++) {
    if (
      i === 1 ||
      i === totalPages ||
      (i >= currentPage - 1 && i <= currentPage + 1)
    ) {
      buttons.push(`
        <button class="pagination-btn ${
          i === currentPage ? "active" : ""
        }" onclick="goToPage(${i})">${i}</button>
      `);
    } else if (i === currentPage - 2 || i === currentPage + 2) {
      buttons.push('<span class="pagination-ellipsis">...</span>');
    }
  }

  buttons.push(`
    <button class="pagination-btn" onclick="goToPage(${currentPage + 1})"
      ${currentPage === totalPages ? "disabled" : ""}>
      Next
    </button>
  `);

  paginationContainer.innerHTML = buttons.join("");
}

function goToPage(page) {
  renderPage(page);
}

document.addEventListener("DOMContentLoaded", () => {
  setupSearchEvents();

  // support pre-filled URL param ?category=
  const defaultCategory = new URLSearchParams(window.location.search).get(
    "category"
  );
  if (defaultCategory) {
    searchInput.value = defaultCategory;
    category = defaultCategory.toLowerCase();
    categoryTitle.textContent = `Category: ${category}`;
    fetchAndRenderCategoryPosts();
  }
});
