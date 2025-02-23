const postsList = document.getElementById("posts-list");

async function View() {
    try {
        const response = await fetch("https://localhost:7136/api/Posts");
        if (response.ok) {
            const posts = await response.json();
            posts.forEach(post => {
                const postItem = document.createElement("div");
                postItem.className = "post-item";
                postItem.innerHTML = `
                    <h3>${post.title}</h3>
                    <p>${post.description}</p>
                    <a href="Update/Title.html?id=${post.id}"><img class="icons" src="title.png"></a>
                    <a href="Update/Description.html?id=${post.id}"><img class="icons" src="description.png"></a>
                    <a href="Delete.html?id=${post.id}"><img class="icons" src="delete.png"></a>
                `;
                postsList.appendChild(postItem);
            });
        } else {
            throw new Error("Failed to fetch posts.");
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Error: " + error.message);
    }
}