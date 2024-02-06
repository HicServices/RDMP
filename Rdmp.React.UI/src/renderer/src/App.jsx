import electronLogo from './assets/electron.svg'
import React, { useEffect, useState } from 'react'
import { ThemeProvider } from '@fluentui/react'
import defaultTheme from '../../theme/defaultTheme'
import { PrimaryButton, makeStyles } from '@fluentui/react'
import { Image, Card } from '@fluentui/react-components'

const style = makeStyles({
  card: {
    width: '100%',
    height: '100%'
  }
})

function App() {
  const styles = style()
  const [catalogues, setCatalogues] = useState(0)
  async function populateCatalogues() {
    const response = await fetch('http://localhost:5067/Catalogues')
    const data = await response.json()
    setCatalogues(data.length)
  }

  useEffect(() => {
    populateCatalogues()
  }, [])

  return (
    <>
      <ThemeProvider theme={defaultTheme}>
        <Card className={styles.card}>
          <Image src={electronLogo} />
          <PrimaryButton>{catalogues} Catalogues</PrimaryButton>
        </Card>
      </ThemeProvider>
    </>
  )
}

export default App
