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
        // === Tirs posés ===
        StandStill,
        SpotUp,

        // === Sur réception ===
        CatchAndShoot,

        // === Sur dribble ===
        PullUp,
        StepBack,
        Hesitation,        

        // === Tirs difficiles ===
        Fadeaway,
        Turnaround,        

        // === Pénétrations ===
        Drive,
        Layup,             
        ReverseLayup,      
        Floater,           
        Eurostep,          
        HopStep,           

        // === Dunks ===
        StandingDunk,      
        DrivingDunk,       
        PosterDunk,


        // === Poste bas ===
        PostUp,
        PostHook,          
        PostFadeaway,

        // === Sans ballon ===
        OffScreen,         
        FlareScreen,

        // === Tactiques ===
        PickAndPop,
        Transition,

        // === Longue distance / désespoir ===
        DeepThree,
        Heave
    }
}