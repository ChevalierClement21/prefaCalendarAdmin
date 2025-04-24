// Fonction pour appliquer les classes CSS responsives en fonction de la taille de l'écran
window.applyResponsiveClasses = function () {
    // Fonction qui sera exécutée lors du redimensionnement de la fenêtre
    function handleResize() {
        // Vérifier si la largeur de la fenêtre est inférieure à 768px (seuil mobile)
        const isMobile = window.innerWidth < 768;
        
        // Récupérer les éléments qui doivent être responsifs
        const totalAmountElement = document.querySelector('.total-amount.with-comparison');
        const sessionSelectorsElement = document.querySelector('.session-selectors');
        const sessionSelectorElements = document.querySelectorAll('.session-selector');
        const comparisonOptionsElement = document.querySelector('.comparison-options');
        
        // Appliquer ou supprimer les classes mobiles en fonction de la taille de l'écran
        if (totalAmountElement) {
            if (isMobile) {
                totalAmountElement.classList.add('with-comparison-mobile');
            } else {
                totalAmountElement.classList.remove('with-comparison-mobile');
            }
        }
        
        if (sessionSelectorsElement) {
            if (isMobile) {
                sessionSelectorsElement.classList.add('session-selectors-mobile');
            } else {
                sessionSelectorsElement.classList.remove('session-selectors-mobile');
            }
        }
        
        sessionSelectorElements.forEach(element => {
            if (isMobile) {
                element.classList.add('session-selector-mobile');
            } else {
                element.classList.remove('session-selector-mobile');
            }
        });
        
        if (comparisonOptionsElement) {
            if (isMobile) {
                comparisonOptionsElement.classList.add('comparison-options-mobile');
            } else {
                comparisonOptionsElement.classList.remove('comparison-options-mobile');
            }
        }
    }
    
    // Exécuter la fonction une première fois pour initialiser les classes
    handleResize();
    
    // Ajouter un écouteur d'événement pour le redimensionnement de la fenêtre
    window.addEventListener('resize', handleResize);
}

// Fonction pour obtenir la largeur de l'écran
window.getScreenWidth = function() {
    return window.innerWidth;
}

// Fonction pour configurer l'écouteur de redimensionnement
let dotNetReference = null;

window.setupResizeListener = function() {
    // Détecter l'instance Blazor
    dotNetReference = DotNet.invokeMethod('prefaCalendarAdmin', 'GetCurrentInstance');
    
    // Ajouter un écouteur d'événement pour le redimensionnement
    window.addEventListener('resize', handleResizeWithDotNet);
}

function handleResizeWithDotNet() {
    if (dotNetReference) {
        dotNetReference.invokeMethodAsync('UpdateScreenWidth', window.innerWidth);
    }
}
