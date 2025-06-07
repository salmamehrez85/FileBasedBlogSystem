const POSTS_PER_PAGE = 2;
let currentPage = 1;
let currentCategory = null;
let currentTag = null;
let searchQuery = "";

const postsContainer = document.getElementById("postsContainer");
const paginationContainer = document.getElementById("pagination");
const searchInput = document.getElementById("searchInput");
const searchButton = document.getElementById("searchButton");

document.addEventListener("DOMContentLoaded", () => {
  loadPosts();
  setupEventListeners();
});

function setupEventListeners() {
  searchButton.addEventListener("click", handleSearch);
  searchInput.addEventListener("keypress", (e) => {
    if (e.key === "Enter") {
      handleSearch();
    }
  });
}

// Fetch posts from the API
async function fetchPosts() {
  try {
    let url;

    if (searchQuery) {
      url = `/posts/search?q=${encodeURIComponent(searchQuery)}`;
    } else if (currentCategory) {
      url = `/posts/category/${encodeURIComponent(currentCategory)}`;
    } else if (currentTag) {
      url = `/posts/tag/${encodeURIComponent(currentTag)}`;
    } else {
      url = `/posts`;
    }

    console.log("Fetching from URL:", url);

    const response = await fetch(url);
    console.log("Response status:", response.status);

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    console.log("Received data:", data);

    let allPosts = [];
    let totalPosts = 0;

    if (Array.isArray(data)) {
      allPosts = data;
      totalPosts = data.length;
    }

    const totalPages = Math.ceil(totalPosts / POSTS_PER_PAGE);
    const startIndex = (currentPage - 1) * POSTS_PER_PAGE;
    const endIndex = startIndex + POSTS_PER_PAGE;
    const paginatedPosts = allPosts.slice(startIndex, endIndex);

    return {
      posts: paginatedPosts,
      totalPages: totalPages,
      totalPosts: totalPosts,
    };
  } catch (error) {
    console.error("Error fetching posts:", error);

    postsContainer.innerHTML = `
      <div class="error-message">
        <h3>Unable to load posts</h3>
        <p>Error: ${error.message}</p>
        <p>Please check your server configuration.</p>
      </div>
    `;

    return { posts: [], totalPages: 0, totalPosts: 0 };
  }
}

async function loadPosts() {
  console.log("Loading posts for page:", currentPage);

  const { posts, totalPages, totalPosts } = await fetchPosts();

  console.log("Load posts result:", {
    postsCount: posts?.length,
    totalPages,
    totalPosts,
    currentPage,
  });

  if (posts && posts.length > 0) {
    displayPosts(posts);
    displayPagination(totalPages);
  } else if (totalPosts === 0) {
    postsContainer.innerHTML = `
      <div class="no-posts">
        <h3>No posts found</h3>
        <p>No posts match your current search criteria.</p>
        ${
          searchQuery || currentCategory || currentTag
            ? '<button onclick="clearFilters()" style="margin-top: 10px; padding: 8px 16px; background: #007bff; color: white; border: none; border-radius: 4px; cursor: pointer;">Clear Filters</button>'
            : ""
        }
      </div>
    `;
    paginationContainer.innerHTML = "";
  } else {
    if (currentPage > 1) {
      currentPage = 1;
      loadPosts();
      return;
    }
    displayPagination(totalPages);
  }
}

function displayPosts(posts) {
  console.log("Displaying posts:", posts.length);

  if (!posts || posts.length === 0) {
    postsContainer.innerHTML = `
      <div class="no-posts">
        <h3>No posts found for this page</h3>
        <p>Try going to a different page or clearing your filters.</p>
      </div>
    `;
    return;
  }

  postsContainer.innerHTML = posts
    .map(
      (post) => `
        <article class="post-card">
            ${
              post.imageUrl
                ? `<img src="${post.imageUrl}" alt="${post.title}" class="post-image" onerror="this.style.display='none'">`
                : ""
            }
            <div class="post-content">
                <h2 class="post-title">
                  <a href="/posts/${
                    post.slug || post.id
                  }" style="text-decoration: none; color: inherit;">
                    ${post.title || "Untitled"}
                  </a>
                </h2>
                <p class="post-excerpt">${
                  post.description || post.excerpt || "No description available"
                }</p>
                <div class="post-meta">
                    <span class="post-date">
                      <i class="fas fa-calendar"></i>
                      ${
                        post.publishedDate || post.createdAt
                          ? new Date(
                              post.publishedDate || post.createdAt
                            ).toLocaleDateString()
                          : "No date"
                      }
                    </span>
                    ${
                      post.categories && post.categories.length > 0
                        ? `<span class="post-category">
                            <i class="fas fa-folder"></i>
                            ${
                              Array.isArray(post.categories)
                                ? post.categories[0]
                                : post.categories
                            }
                          </span>`
                        : post.category
                        ? `<span class="post-category">
                            <i class="fas fa-folder"></i>
                            ${post.category}
                          </span>`
                        : ""
                    }
                    ${
                      post.tags && post.tags.length > 0
                        ? `<div class="post-tags">
                            ${post.tags
                              .slice(0, 3)
                              .map(
                                (tag) =>
                                  `<span class="tag" onclick="filterByTag('${tag}')">${tag}</span>`
                              )
                              .join("")}
                          </div>`
                        : ""
                    }
                </div>
                <div class="post-author">
                  <i class="fas fa-user"></i>
                  By ${post.author || "Unknown"}
                </div>
            </div>
        </article>
    `
    )
    .join("");
}

function displayPagination(totalPages) {
  console.log("displayPagination called with:", { totalPages, currentPage });

  if (totalPages <= 1) {
    console.log("Only 1 or fewer pages, hiding pagination");
    paginationContainer.innerHTML = "";
    return;
  }

  let paginationHTML = '<div class="pagination-controls">';

  // Previous button
  if (currentPage > 1) {
    paginationHTML += `
      <button class="pagination-btn prev-btn" onclick="goToPage(${
        currentPage - 1
      })">
        <i class="fas fa-chevron-left"></i> Previous
      </button>
    `;
  } else {
    paginationHTML += `
      <button class="pagination-btn prev-btn disabled" disabled>
        <i class="fas fa-chevron-left"></i> Previous
      </button>
    `;
  }

  paginationHTML += '<div class="page-numbers">';

  const maxVisiblePages = 5;
  let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
  let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

  // First page and ellipsis
  if (startPage > 1) {
    paginationHTML += `<button class="pagination-btn page-btn" onclick="goToPage(1)">1</button>`;
    if (startPage > 2) {
      paginationHTML += '<span class="pagination-ellipsis">...</span>';
    }
  }

  // Page numbers
  for (let i = startPage; i <= endPage; i++) {
    paginationHTML += `
      <button class="pagination-btn page-btn ${
        i === currentPage ? "active" : ""
      }" 
              onclick="goToPage(${i})">
        ${i}
      </button>
    `;
  }

  // Last page and ellipsis
  if (endPage < totalPages) {
    if (endPage < totalPages - 1) {
      paginationHTML += '<span class="pagination-ellipsis">...</span>';
    }
    paginationHTML += `<button class="pagination-btn page-btn" onclick="goToPage(${totalPages})">${totalPages}</button>`;
  }

  paginationHTML += "</div>";

  // Next button
  if (currentPage < totalPages) {
    paginationHTML += `
      <button class="pagination-btn next-btn" onclick="goToPage(${
        currentPage + 1
      })">
        Next <i class="fas fa-chevron-right"></i>
      </button>
    `;
  } else {
    paginationHTML += `
      <button class="pagination-btn next-btn disabled" disabled>
        Next <i class="fas fa-chevron-right"></i>
      </button>
    `;
  }

  paginationHTML += "</div>";

  paginationHTML += `
    <div class="pagination-info">
      Page ${currentPage} of ${totalPages}
    </div>
  `;

  console.log("Setting pagination HTML for", totalPages, "pages");
  paginationContainer.innerHTML = paginationHTML;
}

function goToPage(page) {
  console.log("goToPage called with:", page);
  if (page < 1) return;

  currentPage = page;
  loadPosts();

  postsContainer.scrollIntoView({ behavior: "smooth", block: "start" });
}

function handleSearch() {
  searchQuery = searchInput.value.trim();
  currentPage = 1;
  currentCategory = null;
  currentTag = null;
  loadPosts();
}

function clearFilters() {
  currentPage = 1;
  currentCategory = null;
  currentTag = null;
  searchQuery = "";
  searchInput.value = "";
  loadPosts();
}
