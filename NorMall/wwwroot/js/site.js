document.addEventListener("DOMContentLoaded", function () {
    console.log('DOMContentLoaded event triggered');

    const categories = document.querySelectorAll("input[type='checkbox']");
    console.log('Total checkboxes found:', categories.length);

    let selectedCategories = JSON.parse(localStorage.getItem('selectedCategories')) || [];
    console.log('Retrieved selected categories from localStorage:', selectedCategories);

    categories.forEach(category => {
        if (selectedCategories.includes(category.value)) {
            category.checked = true;
            console.log('Checkbox checked:', category.value);
        }
        category.addEventListener('change', function () {
            if (this.checked) {
                selectedCategories.push(this.value);
                console.log('Checkbox checked:', this.value);
            } else {
                selectedCategories = selectedCategories.filter(cat => cat !== this.value);
                console.log('Checkbox unchecked:', this.value);
            }
            console.log('Updated selected categories:', selectedCategories);
            localStorage.setItem('selectedCategories', JSON.stringify(selectedCategories));
            console.log('Updated localStorage:', localStorage.getItem('selectedCategories'));
        });
    });
    
    setSearchStringOnRefresh();

    let currentPage = document.getElementById("pageNumberInput").value;
    localStorage.setItem("currentPage", currentPage);
    console.log("Upon page refresh, currentPage is: ", currentPage);
    document.getElementById("pageNumberInput").value = currentPage;
});

function setSearchStringOnRefresh() {
    // Retrieve and set the searchString from localStorage
    let searchString = localStorage.getItem('searchString');
    console.log('Search string is on refresh:', searchString);
    let searchInput = document.getElementById('searchInput');
    if (searchInput && searchString) {
        searchInput.value = searchString;
    }
}

function updateSelectedCategories(categoryName) {
    console.log('Updating selected categories for:', categoryName);

    let selectedCategories = JSON.parse(localStorage.getItem('selectedCategories')) || [];

    if (selectedCategories.includes(categoryName)) {
        selectedCategories = selectedCategories.filter(category => category !== categoryName);
        console.log('Category removed from selected:', categoryName);
    } else {
        selectedCategories.push(categoryName);
        console.log('Category added to selected:', categoryName);
    }

    console.log('Selected Categories:', selectedCategories);
    localStorage.setItem('selectedCategories', JSON.stringify(selectedCategories));
    console.log('Updated localStorage:', localStorage.getItem('selectedCategories'));

    console.log('string sent to server', JSON.stringify(selectedCategories));
    $.ajax({
        type: "POST",
        url: "/Home/SetSelectedCategoriesInSession",
        data: {selectedCategories: JSON.stringify(selectedCategories)},
        success: function (response) {
            console.log("Selected categories updated on server:", response);
        },
        error: function (xhr, textStatus, errorThrown) {
            console.error("Error updating selected categories:", errorThrown);
        }
    });
}

function clearAll() {
    console.log('Before clearing localStorage:', localStorage.getItem('selectedCategories'));
    console.log('Before deleting search string: ', localStorage.getItem('searchString'));

    localStorage.removeItem('selectedCategories');
    localStorage.removeItem('searchString');

    let selectedCategories = JSON.parse(localStorage.getItem('selectedCategories')) || [];

    console.log('Categories string sent to server', JSON.stringify(selectedCategories));

    // AJAX request to clear selected categories on the server
    $.ajax({
        type: "POST",
        url: "/Home/SetSelectedCategoriesInSession",
        data: {selectedCategories: JSON.stringify(selectedCategories)},
    })
        .done(function (response) {
            console.log("Selected categories updated on server:", response);

            // After clearing selected categories on the server, clear the search string
            clearSearchString();
        })
        .fail(function (xhr, textStatus, errorThrown) {
            console.error("Error updating selected categories:", errorThrown);
        });

    let categories = document.querySelectorAll("input[type='checkbox']");

    document.getElementById('searchInput').value = '';

    categories.forEach(category => {
        if (category.checked) {
            category.checked = false;
            console.log(`Checkbox unchecked: ${category.value}`);
        }
    });
}

function clearSearchString() {
    $.ajax({
        type: "POST",
        url: "/Home/SetSearchString",
        data: {inputSearchString: ""},
    })
        .done(function (response) {
            console.log("Searched string updated on server:", response);
        })
        .fail(function (xhr, textStatus, errorThrown) {
            console.error("Error updating searched string:", errorThrown);
        });
}

function handleSearch() {
    var searchString = document.getElementById('searchInput').value; // Get the search string from the input field
    console.log('Searched for:', searchString);

    // Store the search string in localStorage
    localStorage.setItem('searchString', searchString);

    $.ajax({
        type: "POST",
        url: "/Home/SetSearchString",
        data: {inputSearchString: searchString},
        success: function (response) {
            console.log("Searched string updated on server:", response);
        },
        error: function (xhr, textStatus, errorThrown) {
            console.error("Error updating searched string:", errorThrown);
        }
    });
}

function setPageInLocalStorage(maxPage) {
    
    let pageNumber = document.getElementById("pageNumberInput").value;
    
    console.log("Before updating pageNumber: ", localStorage.getItem("currentPage"));
    
    let pageNumberAsInt = parseInt(pageNumber, 10);

    if (maxPage < pageNumberAsInt) {
        pageNumberAsInt = maxPage;
    } else if (pageNumberAsInt < 1) {
        pageNumberAsInt = 1;
    }
    
    console.log("After updating pageNumber: ", pageNumberAsInt);

    localStorage.setItem('currentPage', pageNumberAsInt.toString());

    setPageNumber(pageNumberAsInt);
}

function updatePageInLocalStorage(change) {
    let pageNumber = document.getElementById("pageNumberInput").value;
    if (pageNumber !== '1' || change === 1) {
        console.log("Before updating pageNumber: ", pageNumber);
        change = parseInt(change, 10);
        let pageNumberAsInt = parseInt(pageNumber, 10);
        pageNumberAsInt = pageNumberAsInt + change;
        console.log("After updating pageNumber: ", pageNumberAsInt);

        localStorage.setItem('currentPage', pageNumberAsInt.toString());

        setPageNumber(pageNumberAsInt);
    }
}

function setPageNumber(pageNumber) {

    $.ajax({
        url: '/Home/UpdateCurrentPage',
        type: 'POST',
        data: { currentPage: pageNumber },
        success: function() {
            console.log('Current page updated on the server.', pageNumber);
            location.reload();
        },
        error: function(error) {
            console.error('Error updating current page:', error);
        }
    });
}

function resetPageNumber() {
    localStorage.setItem("currentPage", '1');
    
    $.ajax({
        url: '/Home/UpdateCurrentPage',
        type: 'POST',
        data: { currentPage: 1 },
        success: function() {
            console.log('Current page updated on the server.', localStorage.getItem("currentPage"));
        },
        error: function(error) {
            console.error('Error updating current page:', error);
        }
    });
    
    location.reload();
}

document.addEventListener('DOMContentLoaded', function() {
    let shopLink = document.getElementById('shopLink');

    if (shopLink) {
        shopLink.addEventListener('click', function(event) {
            console.log("Clicked Your Shop button");
            resetPageNumber();
        });
    }
});