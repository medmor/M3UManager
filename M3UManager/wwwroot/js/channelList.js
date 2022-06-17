var ChannelList = {

    indicators: undefined,

    scrollToFiltred(id) {
        var element = document.getElementById(id);
        var topPos = element.offsetTop - 40;
        document.getElementById('channels-container').scrollTop = topPos;
    },

    addIndicators() {
        Array.from(this.indicators).forEach(function (indic) {
            indic.classList.add("hiden")
        });
        if (arguments.length < this.indicators.length) {
            var containerScrollHeight = document.getElementById('channels-container').scrollHeight;
            for (let i = 0; i < arguments.length; i++) {
                var topPos = document.getElementById(arguments[i]).offsetTop;
                this.indicators[i].classList.remove("hiden");
                this.indicators[i].style.top = (topPos / containerScrollHeight) * 100 + 5 + "%"
                console.log(this.indicators[i].style.top, topPos, containerScrollHeight)
            }
        }
    }
}
window.onload = () => {
    ChannelList.indicators = document.getElementsByClassName("indicator");
}
