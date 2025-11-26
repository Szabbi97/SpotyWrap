// Infinite scroll helper for AllLiked page
window.setupInfiniteScroll = function (dotNetHelper) {
    let isScrolling = false;
    
    const handleScroll = () => {
        if (isScrolling) return;
   
        // Check if user scrolled to bottom (with 200px threshold)
      const scrollPosition = window.innerHeight + window.scrollY;
  const threshold = document.documentElement.scrollHeight - 200;
        
   if (scrollPosition >= threshold) {
        isScrolling = true;

     // Call .NET method to load more songs
            dotNetHelper.invokeMethodAsync('LoadMoreSongsFromJS')
           .then(() => {
        setTimeout(() => {
       isScrolling = false;
             }, 1000); // Prevent multiple rapid calls
 })
      .catch((error) => {
           console.error('Error loading more songs:', error);
    isScrolling = false;
         });
        }
    };
    
    window.addEventListener('scroll', handleScroll);
    
    // Return cleanup function
    return {
        dispose: () => {
            window.removeEventListener('scroll', handleScroll);
   }
    };
};

window.disposeInfiniteScroll = function () {
    // Cleanup is handled by the dispose function returned above
};
