import { OhlcDataRecord } from "../ohlc/ohlc-data-record";
import * as d3 from "d3";
import { Observable, Subject } from "rxjs";
import { ChartDragger, Point } from "../chart-dragger";
import { OhlcChartItem } from "./ohlc-chart-item";
import { SvgCore } from "./svg-core";
import { RenderingCtx } from "./rendering-ctx";
import { ChartDimensions } from "./chart-dimensions";


export class ChartCore {
    private _chartItems: OhlcChartItem[];
    private _chartItemsInView: OhlcChartItem[];

    private _chartDragger: ChartDragger = new ChartDragger();
    private _svgCore: SvgCore = new SvgCore();

    private _yScaleRawReversed;
    private _selectedOhlcRecord: OhlcDataRecord;

    private _onOhlcItemSelected: Subject<OhlcDataRecord> = new Subject();
    public onOhlcItemSelected: Observable<OhlcDataRecord> = this._onOhlcItemSelected.asObservable();

    constructor(selector: string) {
        this.initialize(selector);

        this._chartDragger.dragMoved.subscribe((p) => {
            this._svgCore.svgDragMovingHandler(p);
            this.render();
        });
        this._chartDragger.dragStarted.subscribe((p) => {
            this._svgCore.svgDragStartedHandler(p);
        });
        this._chartDragger.dragEnded.subscribe((dp) => {
            this._svgCore.svgDragEndedHandler(dp);
        });
    }

    private _sizing: ChartDimensions = {
        margin: {
            top: 20, right: 50, bottom: 10, left: 0
        },
        chartOffset: {
            max: 100,
            min: 0 // Initialized during runtime.
        },
        bars: {
            width: 10,
            gap: 3,
            maxWidth: 50,
            minWidth: 3
        },
        height: 450,
        width: 0
    };

    public updateSvgWidth() {
        this._svgCore.updateSvgWidth();
        this.render();
    }

    private setZoomLevel(delta: number) {
        let v = this._sizing.bars.width + delta;
        if (v > this._sizing.bars.maxWidth || v < this._sizing.bars.minWidth)
            return;

        let displacement = this.calcZoomChartDisplacement(v);

        this._sizing.bars.width = v;
        this._svgCore.chartOffsetXInitialDiff = null;

        this._svgCore.chartOffsetX -= displacement;
        this.render();
    }

    public initialize(selector: string) {
        this._svgCore.initialize(d3.select(selector), this._sizing);
        this._svgCore.registerSvgHandlers();

        this._svgCore.onSvgMouseMove.subscribe((p) => {
            this.svgMouseMoveHandler(p);
        });

        this._svgCore.onSvgMouseWheel.subscribe((v) => {
            this.setZoomLevel(v / 100);
        });

        this._svgCore.onSvgMouseDown.subscribe((o) => {
            this._chartDragger.dragStart();
        });

        this._svgCore.onSvgMouseUp.subscribe((o) => {
            this._chartDragger.dragEnd();
        });

        this._svgCore.onOhlcItemSelected.subscribe((r) => {
            this._onOhlcItemSelected.next(r);
        })

        this._svgCore.createControls();
    }

    private svgMouseMoveHandler(p: Point) {
        let svgY = p.y - this._sizing.margin.top;
        let crosshairTickerPrice: number = this._yScaleRawReversed ? parseFloat(this._yScaleRawReversed(svgY)) : 0;
        this._svgCore.setCrosshairTickerPrice(crosshairTickerPrice);

        this.selectOhlcRecord(p);

        this._chartDragger.mouseMove(p);
    }

    private selectOhlcRecord(p: Point) {
        if (this._chartItemsInView && this._chartItemsInView.length > 0) {
            let chartOffset = -this._svgCore.chartOffsetX + p.x;

            let dists = this._chartItemsInView.map((x) => {
                return { dist: Math.abs(chartOffset - (x.posX + this._sizing.bars.width / 2)), item: x };
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

    public setData(data: OhlcDataRecord[]) {
        this._chartItems = data.map((x, i) => {
            let r: OhlcChartItem = { ohlc: x, posX: this.getBarPosX(i) };
            return r;
        });

        this._svgCore.chartOffsetX = -this._chartItems[this._chartItems.length - 50].posX;

        this._onOhlcItemSelected.next(this._chartItems[this._chartItems.length - 1].ohlc);
        this.updateSvgWidth();
    }

    private recalcItemsPosX(data: OhlcChartItem[]): OhlcChartItem[] {
        data.forEach((v, i) => {
            v.posX = this.getBarPosX(i);
        });

        let lastItem = data[data.length - 1];
        this._sizing.chartOffset.min = -lastItem.posX + this._sizing.width - 100;

        return data;
    }

    private getBarPosX(i: number): number {
        return i * (this._sizing.bars.width + this._sizing.bars.gap)
    }

    private getInView(data: OhlcChartItem[]): OhlcChartItem[] {
        let self = this;
        let inView = data.filter((record, i) => {
            let startX = -(self._svgCore.chartOffsetX);
            let endX = startX + self._sizing.width - (self._sizing.margin.right + self._sizing.bars.width) - self._sizing.margin.left;
            let currX = record.posX;

            let r = currX >= startX && currX <= endX;

            return r;
        });

        return inView;
    }

    // Calculates center of chart view (SVG view) in chart data coordinates (taking into account chart offset X).
    private calcZoomChartDisplacement(newWidth: number): number {
        let offsetAndWidth = -this._svgCore.chartOffsetX + this._sizing.width;
        let itemsInViewOld = offsetAndWidth / (this._sizing.bars.width + this._sizing.bars.gap);
        let itemsInViewNew = offsetAndWidth / (newWidth + this._sizing.bars.gap);
        let rel = itemsInViewOld / itemsInViewNew;
        let partDiff = (rel * offsetAndWidth - offsetAndWidth) * ((offsetAndWidth - (this._sizing.width - this._chartDragger.mousePos.x)) / offsetAndWidth);
        return partDiff;
    }

    public render() {
        let allData = this.recalcItemsPosX(this._chartItems);

        this._chartItemsInView = this.getInView(allData);
        let dataInView = this._chartItemsInView;

        let yScaleMin = d3.min(dataInView, (d: OhlcChartItem) => { return d.ohlc.low; });
        let yScaleMax = d3.max(dataInView, (d: OhlcChartItem) => { return d.ohlc.high; });
        let yScaleRaw = d3.scaleLinear()
            .domain([yScaleMin, yScaleMax]) // // d3.extent(data, (x: OhlcRecord) => { return x.open; })
            .range([this._sizing.height - this._sizing.margin.bottom - this._sizing.margin.top, 0]);

        let yScaleRawReversed = d3.scaleLinear()
            .domain([this._sizing.height - this._sizing.margin.bottom - this._sizing.margin.top, 0]) // // d3.extent(data, (x: OhlcRecord) => { return x.open; })
            .range([yScaleMin, yScaleMax]);
        this._yScaleRawReversed = yScaleRawReversed;

        // let init = 6320;
        // let forw = yScaleRaw(6320);
        // let back = yScaleRawReversed(forw);
        // console.log(`init: ${init}, forw: ${forw}, back: ${back}`);

        let ctx: RenderingCtx = {
            yScaleMin: yScaleMin,
            yScaleMax: yScaleMax,
            yScaleRaw: yScaleRaw,
            yScale: function (v) {
                return yScaleRaw(v).toFixed(4);
            }
        };

        this._svgCore.render(dataInView, ctx);
    }
}
