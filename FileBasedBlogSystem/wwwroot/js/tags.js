const POSTS_PER_PAGE = 2;
let currentPage = 1;
let tag = "";

const postsContainer = document.getElementById("postsContainer");
const paginationContainer = document.getElementById("pagination");
const tagTitle = document.getElementById("tagTitle");

function getTagFromUrl() {
  const params = new URLSearchParams(window.location.search);
  return params.get("tag");
}

async function fetchTagPosts() {
  try {
    const response = await fetch(`/posts/tag/${encodeURIComponent(tag)}`);
    if (!response.ok) throw new Error("Failed to fetch tag posts");

    const data = await response.json();
    const totalPosts = data.length;
    const totalPages = Math.ceil(totalPosts / POSTS_PER_PAGE);
    const startIndex = (currentPage - 1) * POSTS_PER_PAGE;
    const paginatedPosts = data.slice(startIndex, startIndex + POSTS_PER_PAGE);

    return {
      posts: paginatedPosts,
      totalPages,
    };
  } catch (err) {
    console.error(err);
    postsContainer.innerHTML = `<div class="error-message">Error loading posts</div>`;
    return { posts: [], totalPages: 0 };
  }
}

function displayPosts(posts) {
  if (!posts.length) {
    postsContainer.innerHTML = `<p>No posts under this tag.</p>`;
    return;
  }

  postsContainer.innerHTML = posts
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
    .join("");
}

function displayPagination(totalPages) {
  if (totalPages <= 1) {
    paginationContainer.innerHTML = "";
    return;
  }

  const pages = [];

  pages.push(`
    <button 
      class="pagination-btn"
      onclick="changePage(${Math.max(currentPage - 1, 1)})"
      ${currentPage === 1 ? "disabled" : ""}
    >Prev</button>
  `);

  for (let i = 1; i <= totalPages; i++) {
    if (
      i === 1 ||
      i === totalPages ||
      (i >= currentPage - 1 && i <= currentPage + 1)
    ) {
      pages.push(`
        <button 
          class="pagination-btn ${i === currentPage ? "active" : ""}"
          onclick="changePage(${i})"
        >${i}</button>
      `);
    } else if (i === currentPage - 2 || i === currentPage + 2) {
      pages.push(`<span class="pagination-ellipsis">...</span>`);
    }
  }

  pages.push(`
    <button 
      class="pagination-btn"
      onclick="changePage(${Math.min(currentPage + 1, totalPages)})"
      ${currentPage === totalPages ? "disabled" : ""}
    >Next</button>
  `);

  paginationContainer.innerHTML = pages.join("");
}

function changePage(page) {
  if (page < 1) return;
  currentPage = page;
  loadTagPosts();
}

async function loadTagPosts() {
  const { posts, totalPages } = await fetchTagPosts();
  displayPosts(posts);
  displayPagination(totalPages);
}

document.addEventListener("DOMContentLoaded", () => {
  tag = getTagFromUrl();
  if (!tag) {
    postsContainer.innerHTML = `<p>No tag specified.</p>`;
    return;
  }

  tagTitle.textContent = `Tag: ${tag}`;
  loadTagPosts();
});
