// Native mobile sharing functionality for Everett Eats

window.EverettEats = {
    // Share recipe using native Web Share API or fallback to clipboard
    shareRecipe: async function(title, text, url) {
        try {
            // Check if the Web Share API is supported (primarily mobile browsers)
            if (navigator.share) {
                await navigator.share({
                    title: title,
                    text: text,
                    url: url
                });
                return true;
            } else {
                // Fallback for desktop browsers - copy to clipboard
                await navigator.clipboard.writeText(url);
                alert('Recipe link copied to clipboard!');
                return true;
            }
        } catch (error) {
            console.error('Error sharing recipe:', error);
            // Ultimate fallback - just copy URL to clipboard
            try {
                await navigator.clipboard.writeText(url);
                alert('Recipe link copied to clipboard!');
                return true;
            } catch (clipboardError) {
                console.error('Error copying to clipboard:', clipboardError);
                alert('Unable to share recipe. Please copy the URL manually.');
                return false;
            }
        }
    },

    // Check if native sharing is available
    isNativeSharingAvailable: function() {
        return navigator.share !== undefined;
    }
};
