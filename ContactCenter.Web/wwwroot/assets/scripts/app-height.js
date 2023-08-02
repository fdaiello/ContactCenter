/*
 * appHeight function sets new style property with var(--app-height) with the current window height, this property we can use in the CSS file.
 */
const appHeight = () => {
    const doc = document.documentElement
    doc.style.setProperty('--app-height', `${window.innerHeight}px`)
}
window.addEventListener('resize', appHeight)
appHeight()