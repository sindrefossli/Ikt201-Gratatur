document.addEventListener('DOMContentLoaded', function () {
    // Get carousel elements
    const imageCarousel = document.getElementById('imageCarousel');
    const carouselItems = imageCarousel.getElementsByClassName('carousel-item');
    const prevButton = imageCarousel.getElementsByClassName('carousel-control-prev')[0];
    const nextButton = imageCarousel.getElementsByClassName('carousel-control-next')[0];

    // Initialize variables
    let currentIndex = 0;
    let timer;

    // Function to update the display
    function updateDisplay() {
        for (let i = 0; i < carouselItems.length; i++) {
            carouselItems[i].style.display = i === currentIndex ? 'block' : 'none';
        }
    }

    // Function to move to the next image
    function nextImage() {
        currentIndex = (currentIndex + 1) % carouselItems.length;
        updateDisplay();
    }

    // Function to move to the previous image
    function prevImage() {
        currentIndex = (currentIndex - 1 + carouselItems.length) % carouselItems.length;
        updateDisplay();
    }

    // Function to reset the timer
    function resetTimer() {
        clearInterval(timer);
        timer = setInterval(nextImage, 20000); // Timer is 20 sec
    }

    // Event listeners for buttons
    prevButton.addEventListener('click', function () {
        prevImage();
        resetTimer();
    });

    nextButton.addEventListener('click', function () {
        nextImage();
        resetTimer();
    });

    // Initial display update
    updateDisplay();

    // Start the timer
    resetTimer();
});