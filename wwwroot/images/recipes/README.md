# Recipe Images

Store your recipe images in this directory.

## Image Guidelines

### File Naming Convention
Use the recipe slug for the filename:
- `ultimate-fudgy-brownies.jpg`
- `new-york-vanilla-cheesecake-raspberry-blueberry.jpg`
- `twice-cooked-pork-belly-sticky-glaze.jpg`

### Required Image Specs
- **Dimensions**: Exactly 1200×900px (4:3 aspect ratio)
- **Format**: JPG (preferred) or PNG
- **File size**: 300-500KB after optimization
- **Quality**: 85% JPEG quality recommended

Convert your high-res images to these exact dimensions for consistency across the site.

### How to Reference in recipes.json

Use a relative path from wwwroot:

```json
"imageUrl": "/images/recipes/ultimate-fudgy-brownies.jpg"
```

Or use the example placeholder while you don't have an image:

```json
"imageUrl": "/images/recipes/example-placeholder.svg"
```

### Image Optimization Tips

Before adding images, optimize them:
1. Resize to exactly 1200×900px
2. Compress using tools like TinyPNG, ImageOptim, or Squoosh
3. Convert HEIC/HEIF photos from iPhone to JPG
4. Target 85% JPEG quality for best size/quality balance

### External Images (Unsplash)

You can still use Unsplash URLs if you prefer:
```json
"imageUrl": "https://images.unsplash.com/photo-xxxxx?w=1200&h=900&fit=crop"
```

But hosting your own images gives you:
- Full control over image availability
- Better performance (no external requests)
- No risk of broken links
