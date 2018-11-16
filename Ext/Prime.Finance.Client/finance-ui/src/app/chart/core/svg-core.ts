import { Point } from "../chart-dragger";
import { Subject, Observable } from "rxjs";
import * as d3 from "d3";
import { OhlcChartItem } from "./ohlc-chart-item";
import { OhlcDataRecord } from "../ohlc/ohlc-data-record";
import { RenderingCtx } from "./rendering-ctx";

export class SvgCore {
    private _svg;
    public sizing;

    private _gMain;
    private _gLeftAxis;
    private _selectionCrosshair = { horizontal: null, vertical: null };
    private _gCrosshairPrice;
    private _textCrosshairPrice;
    private _chartItemsInView: OhlcChartItem[];

    private _selectedOhlcRecord: OhlcDataRecord;

    public chartOffsetX: number = 0;

    public yScaleRawReversed; // Remove.
    public selectionAxisPrice; // Remove.

    public chartOffsetXInitialDiff: number = null;

    private _onSvgMouseMove: Subject<Point> = new Subject<Point>();
    public onSvgMouseMove: Observable<Point> = this._onSvgMouseMove.asObservable();

    private _onSvgMouseWheel: Subject<number> = new Subject<number>();
    public onSvgMouseWheel: Observable<number> = this._onSvgMouseWheel.asObservable();

    private _onSvgMouseDown: Subject<object> = new Subject<object>();
    public onSvgMouseDown: Observable<object> = this._onSvgMouseDown.asObservable();

    private _onSvgMouseUp: Subject<object> = new Subject<object>();
    public onSvgMouseUp: Observable<object> = this._onSvgMouseUp.asObservable();

    private _onOhlcItemSelected: Subject<OhlcDataRecord> = new Subject();
    public onOhlcItemSelected: Observable<OhlcDataRecord> = this._onOhlcItemSelected.asObservable();

    public initialize(svg, sizing) {
        this._svg = svg;
        this.sizing = sizing;

        this.setSvgWidth();
    }

    public registerSvgHandlers() {
        this._svg.on("mousemove", () => {
            let [x, y] = d3.mouse(d3.event.currentTarget);
            let p: Point = new Point(x, y);

            this._onSvgMouseMove.next(p);
            this.svgMouseMoveHandler(p);
        });

        this._svg.on("mouseleave", () => {
            this.svgMouseLeaveHandler();
        });

        this._svg.on("mouseenter", () => {
            this.svgMouseEnterHandler();
        });

        this._svg.on("mousewheel", (e) => {
            let scrollValue = d3.event.wheelDeltaY;

            this._onSvgMouseWheel.next(scrollValue);
        });

        this._svg.on("mousedown", () => {
            this.pauseEvent();
            this._onSvgMouseDown.next(null);
        });

        this._svg.on("mouseup", () => {
            this._onSvgMouseUp.next(null);
        });
    }

    public createControls() {
        // Selection line.
        this._selectionCrosshair.vertical = this._svg.append("line")
            .attr("x1", 0)
            .attr("x2", 0)
            .attr("y1", 0)
            .attr("y2", this.sizing.height)
            .attr("stroke-width", 1)
            .attr("stroke-dasharray", "5,5")
            .attr("stroke", "rgba(255, 255, 255, 0.5)");
        this._selectionCrosshair.horizontal = this._svg.append("line")
            .attr("x1", 0)
            .attr("x2", this.sizing.width)
            .attr("y1", 0)
            .attr("y2", 0)
            .attr("stroke-width", 1)
            .attr("stroke-dasharray", "5,5")
            .attr("stroke", "rgba(255, 255, 255, 0.5)");

        // Rect to see SVG's boundaries.
        //this.svg.append("rect").attr("width", "100%").attr("height", "100%").attr("fill", "rgb(30, 30, 30)");


        this._gCrosshairPrice = this._svg.append("g").attr("transform", "translate(-100, 0)");
        this._textCrosshairPrice = this._gCrosshairPrice.append("text").attr("y", 14).attr("x", 2).attr("color", "red").text(6500);
        this._gCrosshairPrice.append("rect").attr("width", this._textCrosshairPrice.node().getBBox().width + 5).attr("height", 18).attr("fill", "#385571");
        this._textCrosshairPrice.raise();

        // Drawing area group.
        this._gMain = this._svg.append("g").attr("transform", `translate(${this.sizing.margin.left}, ${this.sizing.margin.top})`);
        this._gLeftAxis = this._svg.append("g");
    }

    public render(data: OhlcChartItem[], ctx: RenderingCtx) {
        this._chartItemsInView = data;

        // Clear everything.
        this._gMain.selectAll("*").remove();
        this._gLeftAxis.selectAll("*").remove();

        // Move crosshair.
        this._selectionCrosshair.vertical.attr("y2", this.sizing.height);
        this._selectionCrosshair.horizontal.attr("x2", this.sizing.width);

        let yScaleMin: number = ctx.yScaleMin;
        let yScaleMax: number = ctx.yScaleMax;

        // Scales.
        let yScaleRaw = ctx.yScaleRaw;
        let yScale = ctx.yScale;

        // ------------- REMOVE TO CHARTCORE ------------ //
        let yScaleRawReversed = d3.scaleLinear()
            .domain([this.sizing.height - this.sizing.margin.bottom - this.sizing.margin.top, 0]) // // d3.extent(data, (x: OhlcRecord) => { return x.open; })
            .range([yScaleMin, yScaleMax]);
        this.yScaleRawReversed = yScaleRawReversed;
        // ----------------------------------------------- //


        // Populate data.
        let svgData = this._gMain
            .selectAll("g")
            .data(data);

        // Add groups.
        let groups = svgData.enter().append("g")
            .attr("transform", (item: OhlcChartItem, i) => {
                return `translate(${(item.posX + this.chartOffsetX)}, ${yScale(item.ohlc.low)})`;
            })
            .attr("id", (item: OhlcChartItem, i) => {
                return "item-" + item.ohlc.time;
            });

        groups.append("line");
        groups.append("rect");

        // Process lines.
        groups.selectAll("line")
            .attr("transform", (x: OhlcChartItem, i) => {
                return `translate(${(this.sizing.bars.width / 2)}, ${0})`;
            })
            .attr("x1", 0).attr("x2", 0)
            .attr("y1", 0)
            .attr("y2", (x: OhlcChartItem, i) => {
                return (yScale(x.ohlc.high) - yScale(x.ohlc.low));
            })
            .attr("stroke-width", 1)
            .attr("stroke", (o: OhlcChartItem) => {
                return getColor(o.ohlc);
            });


        // Process rects.
        groups.selectAll("rect")
            .attr("width", this.sizing.bars.width)
            .attr("height", (o: OhlcChartItem) => {
                return Math.abs(yScale(o.ohlc.open) - yScale(o.ohlc.close));
            })
            .attr("fill", (o: OhlcChartItem) => {
                return getColor(o.ohlc);
            })
            .attr("x", 0)
            .attr("y", (x: OhlcChartItem, i) => {
                let openLow = yScale(x.ohlc.open) - yScale(x.ohlc.low);
                let closeLow = yScale(x.ohlc.close) - yScale(x.ohlc.low)
                return openLow < closeLow ? openLow : closeLow;
            });

        groups.exit().remove();

        let rightAxis = d3.axisRight(yScaleRaw).ticks(10);
        this._gLeftAxis.attr("transform", `translate(${this.sizing.width - this.sizing.margin.right}, ${this.sizing.margin.top})`).call(rightAxis);


        // Axis.
        //let xAxis = d3.axisLeft(yScaleRaw);
        // svg.append("g").call(xAxis);

        //let gW = g.node().getBBox().width;

        // Line test.
        // function lineTest() {
        //     let line = d3.line()
        //         .x((d: OhlcRecord, i) => {
        //             return i * 10;
        //         })
        //         .y((da: OhlcRecord, i) => {
        //             return yScaleRaw(da.open);
        //         });
        //     svg.append("path")
        //         .attr("stroke-width", 1)
        //         .attr("stroke", "white")
        //         .attr("fill", "transparent")
        //         .attr("d", line(data));
        // }



        // Zoom.
        // svg.call(d3.zoom()
        //     .scaleExtent([1, 10])
        //     //.translateExtent([[-allData[allData.length - 1].posX, 0], [allData[allData.length - 1].posX + 100, 0]])
        //     .on("zoom", () => {
        //         this.onTransformed();
        //     }));

        // TODO: remove.
        function getColor(o: OhlcDataRecord): string {
            return o.open - o.close >= 0 ? "#d81571" : "#4caf0e";
        }
    }

    private pauseEvent() {
        let e = d3.event || window.event;
        if (e.stopPropagation) e.stopPropagation();
        if (e.preventDefault) e.preventDefault();
        e.cancelBubble = true;
        e.returnValue = false;
        return false;
    }

    private svgMouseMoveHandler(p: Point) {
        this.selectionAxisPrice = this.yScaleRawReversed ? parseFloat(this.yScaleRawReversed(p.y)) : 0;

        this._selectionCrosshair.vertical.attr("transform", `translate(${p.x}, 0)`);
        this._selectionCrosshair.horizontal.attr("transform", `translate(0, ${p.y})`);
        this._gCrosshairPrice.attr("transform", `translate(${this.sizing.width - this._gCrosshairPrice.node().getBBox().width}, ${p.y})`);
        this._textCrosshairPrice.text(this.selectionAxisPrice.toFixed(2));

        this.selectOhlcRecord(p);
    }

    private selectOhlcRecord(p: Point) {
        if (this._chartItemsInView && this._chartItemsInView.length > 0) {
            let chartOffset = -this.chartOffsetX + p.x;

            let dists = this._chartItemsInView.map((x) => {
                return { dist: Math.abs(chartOffset - (x.posX + this.sizing.bars.width / 2)), item: x };
            });

            let min = dists[0].dist;
            let prevSelected = this._selectedOhlcRecord ? Object.assign({}, this._selectedOhlcRecord) : null;
            this._selectedOhlcRecord = dists[0].item.ohlc;
            dists.forEach(v => {
                if (v.dist < min) {
                    min = v.dist;
                    this._selectedOhlcRecord = v.item.ohlc;
                }
            });

            if (!prevSelected || prevSelected.time !== this._selectedOhlcRecord.time)
                this._onOhlcItemSelected.next(this._selectedOhlcRecord);
        }
    }

    private svgMouseEnterHandler(): any {
        this._selectionCrosshair.horizontal.style("opacity", 1);
        this._selectionCrosshair.vertical.style("opacity", 1);
    }

    private svgMouseLeaveHandler(): any {
        this._selectionCrosshair.horizontal.style("opacity", 0);
        this._selectionCrosshair.vertical.style("opacity", 0);
    }

    public setSvgWidth() {
        this.sizing.width = this._svg.node().clientWidth;
    }

    public svgDragMovingHandler(p: Point) {
        if (this.chartOffsetXInitialDiff === null) {
            this.chartOffsetXInitialDiff = this.chartOffsetX + p.x;
            //console.log(`Offset: ${}`)
        }
        this.chartOffsetX = p.x + this.chartOffsetXInitialDiff;
    }

    public svgDragStartedHandler(dp: Point) {

    }

    public svgDragEndedHandler(dp: Point) {
        this.chartOffsetXInitialDiff = null;
    }
}
