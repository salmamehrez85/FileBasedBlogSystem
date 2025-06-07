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
  console.log("DOM loaded, initializing...");
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
    let queryParams = new URLSearchParams({
      page: currentPage,
      pageSize: POSTS_PER_PAGE,
    });

    if (searchQuery) {
      url = `/posts/search?q=${encodeURIComponent(searchQuery)}`;
    } else if (currentCategory) {
      url = `/posts/category/${encodeURIComponent(currentCategory)}`;
    } else if (currentTag) {
      url = `/posts/tag/${encodeURIComponent(currentTag)}`;
    } else {
      url = `/posts?${queryParams}`;
    }

    console.log("Fetching from URL:", url);

    const response = await fetch(url);
    console.log("Response status:", response.status);

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    console.log("Received data:", data);

    if (Array.isArray(data)) {
      const posts = data;
      const totalPosts = posts.length;
      const totalPages = Math.ceil(totalPosts / POSTS_PER_PAGE);
      const startIndex = (currentPage - 1) * POSTS_PER_PAGE;
      const endIndex = startIndex + POSTS_PER_PAGE;
      const paginatedPosts = posts.slice(startIndex, endIndex);

      return {
        posts: paginatedPosts,
        totalPages: totalPages,
        totalPosts: totalPosts,
      };
    }
  } catch (error) {
    console.error("Error fetching posts:", error);

    postsContainer.innerHTML = `
      <div class="error-message">
        <h3>Unable to load posts</h3>
        <p>Error: ${error.message}</p>
        <p>Please check your server configuration.</p>
      </div>
    `;

    return { posts: [], totalPages: 0 };
  }
}

async function loadPosts() {
  console.log("Loading posts...");

  const { posts, totalPages } = await fetchPosts();

  if (posts && posts.length > 0) {
    displayPosts(posts);
    displayPagination(totalPages);
  } else {
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
  }
}

// Display posts in the container
function displayPosts(posts) {
  console.log("Displaying posts:", posts.length);

  if (!posts || posts.length === 0) {
    postsContainer.innerHTML = `
      <div class="no-posts">
        <h3>No posts found</h3>
        <p>No posts match your current filters.</p>
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
                    ${
                      post.categories && post.categories.length > 0
                        ? `<span class="post-category">
                            <i class="fas fa-folder"></i>
                            ${post.categories[0]}
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

function handleSearch() {
  searchQuery = searchInput.value.trim();
  currentPage = 1;
  currentCategory = null;
  currentTag = null;
  loadPosts();
}
