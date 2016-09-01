

interface MetisMenuOptions {
    toggle?: boolean;
    doubleTapToGo?: boolean;
    preventDefault?: boolean;
    activeClass?: string;
    collapseClass?: string;
    collapseInClass?: string;
    collapsingClass?: string;
}

interface JQuery {
    metisMenu(options?: MetisMenuOptions): JQuery;
}
