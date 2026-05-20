namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente la situation dans laquelle un tir est pris.
    /// Cette dimension est indépendante de la zone du terrain (CourtZone).
    /// 
    /// Exemple : un tir peut être à la fois "ThreeCornerLeft" (zone)
    /// ET "CatchAndShoot" (contexte). Ces deux dimensions se combinent
    /// pour calculer la probabilité finale de réussite.
    /// </summary>
    public enum ShotContext
    {
        // Tir posé, à l'arrêt, sans pression — situation la plus favorable
        StandStill,

        // Réception et tir immédiat, sans dribble
        // Très utilisé par les shooteurs purs (Klay Thompson, Korver)
        CatchAndShoot,

        // Tir en dribble, sans s'arrêter complètement
        // Demande plus de skill mais permet de créer son tir
        PullUp,

        // Recul avant de tirer pour créer de l'espace face au défenseur
        // Signature de joueurs comme Harden ou Doncic
        StepBack,

        // Tir en se laissant tomber en arrière
        // Difficile mais très peu contrable (Jordan, Dirk, Kobe)
        Fadeaway,

        // Pénétration vers le panier, conclut généralement par layup/dunk
        Drive,

        // Tir depuis le poste bas, dos au panier
        // Spécialité des pivots traditionnels (Olajuwon, Duncan)
        PostUp,

        // Le shooteur ressort après avoir posé un écran (variante du pick and roll)
        PickAndPop,

        // Tir en contre-attaque, défense pas encore replacée
        // Souvent un layup ou un 3 points ouvert
        Transition,

        // Tir à 3 points pris très loin de la ligne (style "logo shot")
        // Bonus de difficulté important sauf pour les snipers d'élite
        DeepThree,

        // Tir désespéré, généralement en fin de quart-temps ou de match
        // Probabilité de réussite très basse, quel que soit le tireur
        Heave
    }
}