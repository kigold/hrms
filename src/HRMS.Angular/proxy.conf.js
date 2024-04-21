module.exports = {
    '/api': {
      target: process.env['services__authapi__https__0'] || process.env['services__authapi__http__0'],
      pathRewrite: {
        '^/api': '',
      },
    },
  };