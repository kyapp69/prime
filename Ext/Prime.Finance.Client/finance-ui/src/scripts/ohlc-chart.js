window.onload = function () {
    Plotly.d3.csv('https://raw.githubusercontent.com/plotly/datasets/master/finance-charts-apple.csv', function (err, rows) {

        function unpack(rows, key) {
            return rows.map(function (row) {
                return row[key];
            });
        }

        var trace = {
            x: unpack(rows, 'Date'),
            close: unpack(rows, 'AAPL.Close'),
            high: unpack(rows, 'AAPL.High'),
            low: unpack(rows, 'AAPL.Low'),
            open: unpack(rows, 'AAPL.Open'),

            // cutomise colors 
            increasing: { line: { color: '#38bf37' } },
            decreasing: { line: { color: '#e82222' } },

            type: 'candlestick',
            xaxis: 'x',
            yaxis: 'y'
        };

        var data = [trace];

        var layout = {
            dragmode: 'pan',
            showlegend: false,
            height: 600,
            xaxis: {
                autorange: true,
                title: 'Date',
                rangeselector: {
                    x: 0,
                    y: 1.2,
                    xanchor: 'left',
                    font: { size: 8 },
                    buttons: [{
                        step: 'month',
                        stepmode: 'backward',
                        count: 1,
                        label: '1 month'
                    }, {
                        step: 'month',
                        stepmode: 'backward',
                        count: 6,
                        label: '6 months'
                    }, {
                        step: 'all',
                        label: 'All dates'
                    }]
                }
            },
            yaxis: {
                autorange: true,
            }
        };

        Plotly.plot('plotly-div', data, layout, {scrollZoom: true, responsive: true});
    });
};
