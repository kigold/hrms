module.exports = {
    '/api': {
      target: process.env['services__authapi__1'],
      pathRewrite: {
        '^/api': '',
      },
    },
  };